using PsjLib.Base;
using PsjLib.Base.Capabilities;
using PsjLib.NVFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.NVFamily;

/// <summary>
/// Base class for NV-series devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> NV identification is probed at a dedicated serial baudrate and then restored to the previous transport setting.</para>
/// </remarks>
public abstract class NVFamilyDevice : PiezoDevice
{
    /// <summary>
    /// Initializes an NV-family device instance.
    /// </summary>
    /// <param name="transportType">Transport backend.</param>
    /// <param name="identifier">Transport identifier.</param>
    protected NVFamilyDevice(TransportType transportType, string identifier)
        : base(transportType, identifier)
    {
        Display = new NVDisplay(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.Display.CmdBrightness] = "light",
        });
    }

    /// <summary>
    /// Probe baudrate used for NV family identification over serial transport.
    /// </summary>
    protected override int SerialBaudrate => 19200;
    /// <summary>
    /// Gets NV-family identifier string expected in startup prompt.
    /// </summary>
    protected abstract string NVFamilyIdentifier { get; }
    /// <summary>
    /// Gets maximum channel count for this model.
    /// </summary>
    protected abstract int MaxChannelCount { get; }
    /// <summary>
    /// Creates channel instance for the specified channel identifier.
    /// </summary>
    protected virtual NVFamilyChannel CreateChannel(int channelId) => new(channelId, WriteChannelAsync);

    /// <inheritdoc/>
    public override string? DeviceId => "NV Family Device";

    /// <summary>
    /// Device-level display capability.
    /// </summary>
    public NVDisplay Display { get; }

    /// <inheritdoc/>
    protected override ISet<string> CacheableCommands { get; } = new HashSet<string>
    {
        "light", "encmode", "enctime", "enclim", "encexp", "encstol", "setk", "monwpa",
        "dspclmin", "dspclmax", "dspvmin", "dspvmax", "unitol", "unitcl",
    };

    /// <inheritdoc/>
    protected override ISet<string> BackupCommands { get; } = new HashSet<string>
    {
        "light", "encmode", "enctime", "enclim", "encexp", "encstol",
    };

    /// <inheritdoc/>
    protected override byte[] FrameDelimiterWrite => TransportProtocol.Cr;
    /// <inheritdoc/>
    protected override byte[] FrameDelimiterRead => TransportProtocol.Xon;

    private static readonly Dictionary<int, ErrorCode> ErrorMap = new()
    {
        [11] = ErrorCode.UnknownCommand,
        [15] = ErrorCode.UnknownChannel,
        [16] = ErrorCode.UnknownChannel,
        [17] = ErrorCode.ParameterMissing,
        [18] = ErrorCode.AdmissibleParameterRangeExceeded,
        [25] = ErrorCode.ActuatorNotConnected,
    };

    /// <inheritdoc/>
    protected override async Task<string?> IsDeviceTypeAsync(TransportProtocol transport)
    {
        var initialBaudrate = transport.GetProperty("baudrate");
        transport.SetProperty("baudrate", SerialBaudrate);

        // Try to connect 3 times to account for potential garbage in device input buffer
        try
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                await transport.WriteAsync("\r").ConfigureAwait(false);
                var msg = await transport.ReadUntilAsync(FrameDelimiterRead, 5.0).ConfigureAwait(false);
            
                if (msg.Contains(NVFamilyIdentifier + ">", StringComparison.Ordinal))
                {
                    return DeviceId;
                }
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            if (initialBaudrate is not null)
            {
                transport.SetProperty("baudrate", initialBaudrate);
            }
        }

        return null;
    }

    /// <inheritdoc/>
    protected override Task DiscoverChannelsAsync()
    {
        ChannelsInternal.Clear();
        for (var channelId = 0; channelId < MaxChannelCount; channelId++)
        {
            ChannelsInternal[channelId] = CreateChannel(channelId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override void HandleError(string response)
    {
        if (!response.StartsWith("ErrorCode", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var parts = response.Split(',', 2);
        if (parts.Length < 2)
        {
            ErrorCode.ErrorNotSpecified.RaiseError(response);
        }

        if (!int.TryParse(parts[1].Trim('\x01', '\n', '\r', '\0'), out var code))
        {
            ErrorCode.ErrorNotSpecified.RaiseError(response);
        }

        if (ErrorMap.TryGetValue(code, out var mapped))
        {
            mapped.RaiseError(response);
            return;
        }

        ErrorCode.ErrorNotSpecified.RaiseError(response);
    }

    /// <summary>
    /// Routes NV channel commands and omits channel prefix for global commands.
    /// </summary>
    internal override Task<IReadOnlyList<string>> WriteChannelAsync(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null)
    {
        var command = cmd.Split(',')[0];
        if (channelId is not null && NVFamilyChannel.GlobalCommands.Contains(command))
        {
            return base.WriteChannelAsync(null, cmd, parameters);
        }

        return base.WriteChannelAsync(channelId, cmd, parameters);
    }

}
