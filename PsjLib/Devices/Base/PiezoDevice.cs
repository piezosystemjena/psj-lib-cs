using System.Globalization;
using System.Text;
using PsjLib.Transport;

namespace PsjLib.Base;

/// <summary>
/// Base class for all supported piezo amplifier devices.
/// </summary>
/// <remarks>
/// This type encapsulates transport setup, command serialization, response parsing,
/// command caching, and channel discovery for derived device implementations.
/// <para><b>Notes:</b> Discovery behavior and channel topology are device-family specific and are implemented by derived classes.</para>
/// </remarks>
public abstract class PiezoDevice : IAsyncDisposable
{
    /// <summary>
    /// Command response cache used to optimize repeated reads.
    /// </summary>
    protected readonly CommandCache Cache;
    /// <summary>
    /// Synchronizes transport access so command/response pairs remain ordered.
    /// </summary>
    protected readonly SemaphoreSlim DeviceLock = new(1, 1);
    /// <summary>
    /// Active transport used to communicate with the physical device.
    /// </summary>
    protected readonly TransportProtocol Transport;
    /// <summary>
    /// Mutable channel map filled during connection and discovery.
    /// </summary>
    protected readonly Dictionary<int, PiezoChannel> ChannelsInternal = new();

    /// <summary>
    /// Initializes a new device abstraction bound to a transport endpoint.
    /// </summary>
    /// <param name="transportType">Transport backend to use.</param>
    /// <param name="identifier">Transport-specific address (for example COM port or IP/MAC).</param>
    protected PiezoDevice(TransportType transportType, string identifier)
    {
        Transport = TransportFactory.FromTransportType(transportType, identifier);
        Cache = new CommandCache(CacheableCommands);
        Transport.RxDelimiter = FrameDelimiterRead;

        Transport.SetProperty("baudrate", SerialBaudrate);
    }

    /// <summary>
    /// Gets the model identifier for this device type.
    /// </summary>
    public virtual string? DeviceId => null;
    /// <summary>
    /// Gets whether this model exposes exactly one logical channel.
    /// </summary>
    public virtual bool SingleChannel => false;
    /// <summary>
    /// Gets commands that may be cached between reads.
    /// </summary>
    protected virtual ISet<string> CacheableCommands => new HashSet<string>();
    /// <summary>
    /// Gets global (device-level) commands included in backup operations.
    /// </summary>
    protected virtual ISet<string> BackupCommands => new HashSet<string>();
    /// <summary>
    /// Gets default command timeout in seconds.
    /// </summary>
    protected virtual double DefaultTimeoutSecs => 0.6;
    /// <summary>
    /// The default Serial baudrate.
    /// </summary>
    protected virtual int SerialBaudrate => 115200;
    /// <summary>
    /// Gets delimiter bytes appended to outgoing command frames.
    /// </summary>
    protected virtual byte[] FrameDelimiterWrite => TransportProtocol.Crlf;
    /// <summary>
    /// Gets delimiter bytes expected for incoming device frames.
    /// </summary>
    protected virtual byte[] FrameDelimiterRead => TransportProtocol.Crlf;

    /// <summary>
    /// Gets discovered channels keyed by channel identifier.
    /// </summary>
    public IReadOnlyDictionary<int, PiezoChannel> Channels => ChannelsInternal;

    /// <summary>
    /// Gets current transport and model metadata for this device instance.
    /// </summary>
    public DeviceInfo DeviceInfo => new(Transport.GetInfo(), DeviceId);

    /// <summary>
    /// Discovers connected devices compatible with <typeparamref name="TDevice"/>.
    /// </summary>
    /// <typeparam name="TDevice">Concrete or abstract device base type to discover.</typeparam>
    /// <param name="flags">Transport discovery options.</param>
    /// <returns>Instantiated device wrappers for discovered devices.</returns>
    public static async Task<IReadOnlyList<TDevice>> DiscoverDevicesAsync<TDevice>(
        DiscoverFlags flags = DiscoverFlags.AllInterfaces) where TDevice : PiezoDevice
    {
        // If TDevice is abstract, we need to discover devices for all non-abstract subclasses of TDevice and aggregate results
        if (typeof(TDevice).IsAbstract)
        {
            return await DiscoverDevicesAbstractClassAsync<TDevice>(flags).ConfigureAwait(false);
        }

        TDevice tmp = (TDevice)Activator.CreateInstance(typeof(TDevice), TransportType.Serial, string.Empty)!;
        var discovered = await DeviceDiscovery.DiscoverDevicesAsync(tmp.IsDeviceTypeAsync, flags).ConfigureAwait(false);
        return discovered
            .Select(DeviceFactory.FromDetectedDevice)
            .OfType<TDevice>()
            .ToList();
    }

    /// <summary>
    /// Discovers all non-abstract subclasses of an abstract device base type.
    /// </summary>
    /// <typeparam name="TDevice">Abstract base type whose concrete descendants should be discovered.</typeparam>
    /// <param name="flags">Transport discovery options.</param>
    /// <returns>Aggregated discovered devices for all matching concrete subtypes.</returns>
    public static async Task<IReadOnlyList<TDevice>> DiscoverDevicesAbstractClassAsync<TDevice>(
        DiscoverFlags flags = DiscoverFlags.AllInterfaces) where TDevice : PiezoDevice
    {
        List<TDevice> results = [];

        var childClasses = typeof(TDevice).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(TDevice).IsAssignableFrom(t));

        // Discover devices for each of the child classes of the abstract class, and aggregate results
        foreach (var child in childClasses)
        {
            // Call the DiscoverDevicesAsync method for the child class using reflection, since we don't know the type at compile time
            var method = typeof(PiezoDevice)
                .GetMethods()
                .First(m => m.Name == nameof(DiscoverDevicesAsync) && m.IsGenericMethodDefinition);

            var genericMethod = method.MakeGenericMethod(child);
            var taskObj = (Task?)genericMethod.Invoke(null, new object[] { flags });

            // If for some reason the invoked method does not return a Task, skip it
            if (taskObj is null)
            {
                continue;
            }

            await taskObj.ConfigureAwait(false);

            // Await the returned Task and add results to aggregate list
            var childResult = (IReadOnlyList<TDevice>?)taskObj
                .GetType()
                .GetProperty("Result")?
                .GetValue(taskObj);

            if (childResult is null)
            {
                continue;
            }

            results.AddRange(childResult.OfType<TDevice>());
        }

        return results;
    }

    /// <summary>
    /// Connects transport, verifies device type, and discovers channels.
    /// </summary>
    /// <param name="autoAdjustCommParams">When <see langword="true"/>, transport-specific communication tuning may be applied.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Automatic communication-parameter adjustment may increase connection time for some network transports.</para>
    /// </remarks>
    public virtual async Task ConnectAsync(bool autoAdjustCommParams = true)
    {
        if (Transport.IsConnected)
        {
            return;
        }

        await Transport.ConnectAsync(autoAdjustCommParams).ConfigureAwait(false);
        var match = await IsDeviceTypeAsync(Transport).ConfigureAwait(false);
        if (match is null)
        {
            await Transport.CloseAsync().ConfigureAwait(false);
            throw new DeviceUnavailableException($"Device type mismatch. Expected {DeviceId}");
        }

        await DiscoverChannelsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether a transport endpoint matches this concrete device type.
    /// </summary>
    /// <param name="transport">Connected transport to probe.</param>
    /// <returns>Model identifier when matched; otherwise <see langword="null"/>.</returns>
    protected virtual Task<string?> IsDeviceTypeAsync(TransportProtocol transport)
    {
        return Task.FromResult<string?>(null);
    }
 
    /// <summary>
    /// Discovers and initializes channel objects for this model.
    /// </summary>
    protected abstract Task DiscoverChannelsAsync();

    /// <summary>
    /// Parses a raw device response into value tokens.
    /// </summary>
    /// <param name="response">Raw response frame without transport delimiter.</param>
    /// <returns>Parsed response values.</returns>
    /// <exception cref="DeviceError">Raised when response indicates a device error.</exception>
    protected virtual IReadOnlyList<string> ParseResponse(string response)
    {
        HandleError(response);
        
        var parts = response.Split(',', 2);
        if (parts.Length <= 1)
        {
            return [];
        }

        return parts[1]
            .Split(',')
            .Select(static p => p.Trim('\x01', '\n', '\r', '\0'))
            .ToList();
    }

    protected virtual void HandleError(string response)
    {
        if (response.StartsWith("error", StringComparison.OrdinalIgnoreCase))
        {
            RaiseError(response);
        }
    }

    /// <summary>
    /// Parses a device error response and throws a typed exception.
    /// </summary>
    /// <param name="response">Raw error response.</param>
    protected virtual void RaiseError(string response)
    {
        var parts = response.Split(',', 2);
        if (parts.Length < 2)
        {
            ErrorCode.ErrorNotSpecified.RaiseError(response);
        }

        if (!int.TryParse(parts[1].Trim('\x01', '\n', '\r', '\0'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
        {
            ErrorCode.ErrorNotSpecified.RaiseError(response);
        }

        var mapped = Enum.IsDefined(typeof(ErrorCode), code)
            ? (ErrorCode)code
            : ErrorCode.ErrorNotSpecified;

        mapped.RaiseError(response);
    }

    /// <summary>
    /// Writes a command scoped to a channel and normalizes channel-prefixed responses.
    /// </summary>
    /// <param name="channelId">Channel index or <see langword="null"/> for global commands.</param>
    /// <param name="cmd">Command token or command with fixed parameters.</param>
    /// <param name="parameters">Additional command parameters.</param>
    /// <returns>Parsed response values, without echoed channel number when present.</returns>
    internal virtual async Task<IReadOnlyList<string>> WriteChannelAsync(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null)
    {
        var cmdParts = cmd.Split(',');
        var fullCmd = channelId is null ? cmdParts[0] : $"{cmdParts[0]},{channelId}";

        List<object?>? effective = null;
        if (cmdParts.Length > 1 || parameters is not null)
        {
            effective = [];
            if (cmdParts.Length > 1)
            {
                effective.AddRange(cmdParts.Skip(1));
            }

            if (parameters is not null)
            {
                effective.AddRange(parameters);
            }
        }

        var result = await WriteAsync(fullCmd, effective).ConfigureAwait(false);
        if (channelId is not null && result.Count > 0)
        {
            return result.Skip(1).ToList();
        }

        return result;
    }

    /// <summary>
    /// Resolves a capability command token to a protocol command and executes it.
    /// </summary>
    /// <param name="deviceCommands">Capability command map.</param>
    /// <param name="command">Capability command token.</param>
    /// <param name="parameters">Optional command arguments.</param>
    /// <returns>Parsed response values, or an empty list if command is unmapped.</returns>
    internal async Task<IReadOnlyList<string>> CapabilityWriteAsync(
        IReadOnlyDictionary<string, string> deviceCommands,
        string command,
        IReadOnlyList<object?>? parameters = null)
    {
        if (!deviceCommands.TryGetValue(command, out var mapped))
        {
            return [];
        }

        return await WriteAsync(mapped, parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a command with optional arguments and returns parsed values.
    /// </summary>
    /// <param name="cmd">Base command name.</param>
    /// <param name="parameters">Optional write values. If <see langword="null"/>, a read command is assumed.</param>
    /// <param name="timeoutSecs">Optional custom timeout in seconds.</param>
    /// <returns>Parsed response values.</returns>
    public async Task<IReadOnlyList<string>> WriteAsync(string cmd, IReadOnlyList<object?>? parameters = null, double? timeoutSecs = null)
    {
        if (parameters is null)
        {
            return await ReadWithCacheAsync(cmd, timeoutSecs ?? DefaultTimeoutSecs).ConfigureAwait(false);
        }

        return await WriteWithCacheAsync(cmd, parameters, timeoutSecs ?? DefaultTimeoutSecs).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<string>> ReadWithCacheAsync(string cmd, double timeoutSecs)
    {
        var cached = Cache.Get(cmd);
        if (cached is not null)
        {
            return cached;
        }

        var values = (await WriteAndParseAsync(cmd, timeoutSecs).ConfigureAwait(false)).Select(static x => x.TrimEnd()).ToList();
        Cache.Set(cmd, values);
        return values;
    }

    private async Task<IReadOnlyList<string>> WriteWithCacheAsync(string cmd, IReadOnlyList<object?> values, double timeoutSecs)
    {
        var serialized = values.Select(static v => v switch
        {
            null => string.Empty,
            bool b => b ? "1" : "0",
            Enum e => Convert.ToInt32(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => v?.ToString() ?? string.Empty,
        });

        var response = await WriteAndParseAsync($"{cmd},{string.Join(',', serialized)}", timeoutSecs).ConfigureAwait(false);
        Cache.Invalidate(cmd);
        return response;
    }

    /// <summary>
    /// Sends a command frame and parses the response.
    /// </summary>
    /// <param name="cmd">Fully serialized command frame without transport delimiter.</param>
    /// <param name="timeoutSecs">Read timeout in seconds.</param>
    /// <returns>Parsed response values.</returns>
    protected async Task<IReadOnlyList<string>> WriteAndParseAsync(string cmd, double timeoutSecs)
    {
        var response = await WriteRawAsync(cmd, timeoutSecs).ConfigureAwait(false);
        return ParseResponse(response);
    }

    /// <summary>
    /// Sends a raw command to the transport and returns the unparsed response payload.
    /// </summary>
    /// <param name="cmd">Command body to send (without line/frame delimiter).</param>
    /// <param name="timeoutSecs">Optional timeout override in seconds.</param>
    /// <param name="rxDelimiter">Optional response delimiter override.</param>
    /// <returns>Raw response payload returned by the transport.</returns>
    public virtual async Task<string> WriteRawAsync(string cmd, double? timeoutSecs = null, byte[]? rxDelimiter = null)
    {
        if (!Transport.IsConnected)
        {
            throw new DeviceUnavailableException("Device is not connected");
        }

        await DeviceLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var frame = cmd + Encoding.Latin1.GetString(FrameDelimiterWrite);
            await Transport.WriteAsync(frame).ConfigureAwait(false);
            return await Transport.ReadUntilAsync(rxDelimiter ?? FrameDelimiterRead, timeoutSecs ?? DefaultTimeoutSecs).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not DeviceError and not ProtocolException)
        {
            throw new DeviceUnavailableException("Failed to write/read from device", ex);
        }
        finally
        {
            DeviceLock.Release();
        }
    }

    /// <summary>
    /// Reads and returns a backup snapshot of global and optionally per-channel command values.
    /// </summary>
    /// <param name="backupList">Optional explicit list of global commands to backup.</param>
    /// <param name="backupChannels">Whether to include channel command snapshots.</param>
    /// <returns>Dictionary mapping command identifiers to captured values.</returns>
    public async Task<Dictionary<string, IReadOnlyList<string>>> BackupAsync(
        IReadOnlyCollection<string>? backupList = null,
        bool backupChannels = true)
    {
        ClearCommandCache();
        IReadOnlyCollection<string> commands = backupList ?? BackupCommands.ToList();
        var backup = new Dictionary<string, IReadOnlyList<string>>();

        foreach (var cmd in commands)
        {
            backup[cmd] = await WriteAsync(cmd).ConfigureAwait(false);
        }

        if (!backupChannels)
        {
            return backup;
        }

        foreach (var channel in ChannelsInternal.Values)
        {
            var channelBackup = await channel.BackupAsync().ConfigureAwait(false);
            foreach (var kv in channelBackup)
            {
                backup[$"{kv.Key},{channel.Id}"] = kv.Value;
            }
        }

        return backup;
    }

    /// <summary>
    /// Restores command values from a backup snapshot.
    /// </summary>
    /// <param name="backup">Backup dictionary previously produced by <see cref="BackupAsync(IReadOnlyCollection{string}?, bool)"/>.</param>
    public async Task RestoreAsync(IReadOnlyDictionary<string, IReadOnlyList<string>> backup)
    {
        foreach (var kv in backup)
        {
            await WriteAsync(kv.Key, kv.Value.Cast<object?>().ToList()).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Clears all cached command responses.
    /// </summary>
    public void ClearCommandCache() => Cache.Clear();

    /// <summary>
    /// Enables or disables command response caching.
    /// </summary>
    /// <param name="enable">New cache state.</param>
    public void EnableCommandCache(bool enable)
    {
        Cache.Enabled = enable;
        if (!enable)
        {
            Cache.Clear();
        }
    }

    /// <summary>
    /// Closes transport connection and clears command cache.
    /// </summary>
    public virtual async Task CloseAsync()
    {
        await Transport.CloseAsync().ConfigureAwait(false);
        ClearCommandCache();
    }

    /// <summary>
    /// Asynchronously disposes the device and underlying synchronization resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        DeviceLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
