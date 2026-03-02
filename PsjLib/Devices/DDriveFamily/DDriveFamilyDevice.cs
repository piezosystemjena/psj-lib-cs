using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Base class for piezosystem jena d-Drive family devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Family variants share protocol behavior but differ in channel topology (multi-channel d-Drive versus single-channel PSJ 30DV).</para>
/// <para><b>Notes:</b> Channel IDs use the same numeric namespace even when not all indices are populated in hardware.</para>
/// </remarks>
public abstract class DDriveFamilyDevice(TransportType transportType, string identifier)
    : PiezoDevice(transportType, identifier)
{
    /// <summary>
    /// Gets the identifier fragment expected in startup banner for this model.
    /// </summary>
    protected abstract string DDriveIdentifier { get; }

    /// <inheritdoc/>
    public override string? DeviceId => "d-Drive Family Device";

    /// <inheritdoc/>
    protected override ISet<string> CacheableCommands { get; } = new HashSet<string>
    {
        "acdescr", "acolmas", "acclmas", "set", "fan", "modon", "monsrc", "cl", "sr", "pcf", "errlpf",
        "elpor", "kp", "ki", "kd", "tf", "notchon", "notchf", "notchb", "lpon", "lpf", "gfkt", "gasin",
        "gosin", "gfsin", "gatri", "gotri", "gftri", "gstri", "garec", "gorec", "gfrec", "gsrec", "ganoi",
        "gonoi", "gaswe", "goswe", "gtswe", "sct", "trgss", "trgse", "trgsi", "trglen", "trgedge", "trgsrc",
        "trgos", "recstride", "bright",
    };

    /// <inheritdoc/>
    protected override double DefaultTimeoutSecs => 0.5;
    /// <inheritdoc/>
    protected override byte[] FrameDelimiterRead => TransportProtocol.Xon;

    private static readonly Dictionary<string, ErrorCode> ErrorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["command not found"] = ErrorCode.UnknownCommand,
        ["command mismatch"] = ErrorCode.CommandParameterCountExceeded,
        [" not present"] = ErrorCode.UnknownChannel,
        ["unit not available"] = ErrorCode.ActuatorNotConnected,
    };

    private static readonly Dictionary<string, byte[]> FrameDelimiterMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ktemp"] = TransportProtocol.Cr,
        ["m"] = TransportProtocol.Cr,
        ["u"] = TransportProtocol.Cr,
        ["modon"] = TransportProtocol.Cr,
        ["monsrc"] = TransportProtocol.Cr,
        ["pcf"] = TransportProtocol.Cr,
        ["errlpf"] = TransportProtocol.Cr,
        ["elpor"] = TransportProtocol.Cr,
        ["sr"] = TransportProtocol.Cr,
        ["kp"] = TransportProtocol.Cr,
        ["ki"] = TransportProtocol.Cr,
        ["kd"] = TransportProtocol.Cr,
        ["tf"] = TransportProtocol.Cr,
        ["notchon"] = TransportProtocol.Cr,
        ["notchf"] = TransportProtocol.Cr,
        ["notchb"] = TransportProtocol.Cr,
        ["lpon"] = TransportProtocol.Cr,
        ["lpf"] = TransportProtocol.Cr,
        ["gfkt"] = TransportProtocol.Cr,
        ["gasin"] = TransportProtocol.Cr,
        ["gosin"] = TransportProtocol.Cr,
        ["gfsin"] = TransportProtocol.Cr,
        ["gatri"] = TransportProtocol.Cr,
        ["gotri"] = TransportProtocol.Cr,
        ["gftri"] = TransportProtocol.Cr,
        ["gstri"] = TransportProtocol.Cr,
        ["garec"] = TransportProtocol.Cr,
        ["gorec"] = TransportProtocol.Cr,
        ["gfrec"] = TransportProtocol.Cr,
        ["gsrec"] = TransportProtocol.Cr,
        ["ganoi"] = TransportProtocol.Cr,
        ["gonoi"] = TransportProtocol.Cr,
        ["gaswe"] = TransportProtocol.Cr,
        ["goswe"] = TransportProtocol.Cr,
        ["gtswe"] = TransportProtocol.Cr,
        ["sct"] = TransportProtocol.Cr,
        ["trgss"] = TransportProtocol.Cr,
        ["trgse"] = TransportProtocol.Cr,
        ["trgsi"] = TransportProtocol.Cr,
        ["trglen"] = TransportProtocol.Cr,
        ["trgedge"] = TransportProtocol.Cr,
        ["trgsrc"] = TransportProtocol.Cr,
        ["trgos"] = TransportProtocol.Cr,
    };

    /// <inheritdoc/>
    protected override async Task<string?> IsDeviceTypeAsync(TransportProtocol transport)
    {
        try
        {
            await transport.WriteAsync("\r\n").ConfigureAwait(false);
            var msg = await transport.ReadMessageAsync().ConfigureAwait(false);
            return msg.Contains(DDriveIdentifier + " V", StringComparison.Ordinal) ? DeviceId : null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<string> ParseResponse(string response)
    {
        foreach (var kv in ErrorMap)
        {
            if (response.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                kv.Value.RaiseError(response);
            }
        }

        return base.ParseResponse(response);
    }

    /// <inheritdoc/>
    public override async Task<string> WriteRawAsync(string cmd, double? timeoutSecs = null, byte[]? rxDelimiter = null)
    {
        var isRead = (SingleChannel && cmd.Count(c => c == ',') == 0) 
            || (!SingleChannel && cmd.Count(c => c == ',') <= 1);

        if (isRead || cmd.StartsWith("m,", StringComparison.OrdinalIgnoreCase) || cmd.StartsWith("u,", StringComparison.OrdinalIgnoreCase))
        {
            var rawCmd = cmd.Split(',')[0].ToLowerInvariant();

            if (FrameDelimiterMap.TryGetValue(rawCmd, out var delimiter))
            {
                rxDelimiter = delimiter;
            }
        }

        return await base.WriteRawAsync(cmd, timeoutSecs, rxDelimiter).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets typed d-Drive family channels keyed by channel identifier.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Consumers should iterate available keys instead of assuming all possible channel IDs exist.</para>
    /// </remarks>
    public new IReadOnlyDictionary<int, DDriveFamilyChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (DDriveFamilyChannel)kv.Value);
}
