using System.Globalization;
using System.Text;
using PsjLib.Transport;

namespace PsjLib.Base;

public abstract class PiezoDevice : IAsyncDisposable
{
    protected readonly CommandCache Cache;
    protected readonly SemaphoreSlim DeviceLock = new(1, 1);
    protected readonly TransportProtocol Transport;
    protected readonly Dictionary<int, PiezoChannel> ChannelsInternal = new();

    protected PiezoDevice(TransportType transportType, string identifier)
    {
        Transport = TransportFactory.FromTransportType(transportType, identifier);
        Cache = new CommandCache(CacheableCommands);
        Transport.RxDelimiter = FrameDelimiterRead;
    }

    public virtual string? DeviceId => null;
    public virtual bool SingleChannel => false;
    protected virtual ISet<string> CacheableCommands => new HashSet<string>();
    protected virtual ISet<string> BackupCommands => new HashSet<string>();
    protected virtual double DefaultTimeoutSecs => 0.6;
    protected virtual byte[] FrameDelimiterWrite => TransportProtocol.Crlf;
    protected virtual byte[] FrameDelimiterRead => TransportProtocol.Crlf;

    public IReadOnlyDictionary<int, PiezoChannel> Channels => ChannelsInternal;

    public DeviceInfo DeviceInfo => new(Transport.GetInfo(), DeviceId);

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

    protected static async Task<string?> IsAnyRegisteredTypeAsync(TransportProtocol transport)
    {
        foreach (var kv in DeviceModelRegistry.Registry)
        {
            var device = kv.Value(transport.TransportType, transport.Identifier);
            var match = await device.IsDeviceTypeAsync(transport).ConfigureAwait(false);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    protected virtual Task<string?> IsDeviceTypeAsync(TransportProtocol transport)
    {
        Console.WriteLine($"HERE");
        return Task.FromResult<string?>(null);
    }
 
    protected abstract Task DiscoverChannelsAsync();

    protected virtual IReadOnlyList<string> ParseResponse(string response)
    {
        if (response.StartsWith("error", StringComparison.OrdinalIgnoreCase))
        {
            RaiseError(response);
        }

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

    protected async Task<IReadOnlyList<string>> WriteAndParseAsync(string cmd, double timeoutSecs)
    {
        var response = await WriteRawAsync(cmd, timeoutSecs).ConfigureAwait(false);
        return ParseResponse(response);
    }

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

    public async Task RestoreAsync(IReadOnlyDictionary<string, IReadOnlyList<string>> backup)
    {
        foreach (var kv in backup)
        {
            await WriteAsync(kv.Key, kv.Value.Cast<object?>().ToList()).ConfigureAwait(false);
        }
    }

    public void ClearCommandCache() => Cache.Clear();

    public void EnableCommandCache(bool enable)
    {
        Cache.Enabled = enable;
        if (!enable)
        {
            Cache.Clear();
        }
    }

    public virtual async Task CloseAsync()
    {
        await Transport.CloseAsync().ConfigureAwait(false);
        ClearCommandCache();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        DeviceLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
