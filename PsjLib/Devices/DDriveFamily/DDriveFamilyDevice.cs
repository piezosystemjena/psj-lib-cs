using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

public abstract class DDriveFamilyDevice(TransportType transportType, string identifier)
    : PiezoDevice(transportType, identifier)
{
    protected abstract string DDriveIdentifier { get; }

    public override string? DeviceId => "d-Drive Family Device";

    protected override ISet<string> CacheableCommands { get; } = new HashSet<string>
    {
        "acdescr", "acolmas", "acclmas", "set", "fan", "modon", "monsrc", "cl", "sr", "pcf", "errlpf",
        "elpor", "kp", "ki", "kd", "tf", "notchon", "notchf", "notchb", "lpon", "lpf", "gfkt", "gasin",
        "gosin", "gfsin", "gatri", "gotri", "gftri", "gstri", "garec", "gorec", "gfrec", "gsrec", "ganoi",
        "gonoi", "gaswe", "goswe", "gtswe", "sct", "trgss", "trgse", "trgsi", "trglen", "trgedge", "trgsrc",
        "trgos", "recstride", "bright",
    };

    protected override double DefaultTimeoutSecs => 0.5;
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

    public new IReadOnlyDictionary<int, DDriveFamilyChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (DDriveFamilyChannel)kv.Value);
}
