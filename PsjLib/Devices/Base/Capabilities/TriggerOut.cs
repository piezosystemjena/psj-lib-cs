namespace PsjLib.Base.Capabilities;

public enum TriggerEdge
{
    Rising = 0,
    Falling = 1,
}

public enum TriggerDataSource
{
    Position = 0,
    Voltage = 1,
}

public class TriggerOut(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdStart = "TRIGGER_OUT_START";
    public const string CmdStop = "TRIGGER_OUT_STOP";
    public const string CmdInterval = "TRIGGER_OUT_INTERVAL";
    public const string CmdLength = "TRIGGER_OUT_LENGTH";
    public const string CmdEdge = "TRIGGER_OUT_EDGE";
    public const string CmdSrc = "TRIGGER_OUT_SOURCE";

    public virtual async Task SetAsync(
        double? startValue = null,
        double? stopValue = null,
        double? interval = null,
        int? length = null,
        TriggerEdge? edge = null,
        TriggerDataSource? src = null)
    {
        if (startValue is not null)
            _ = await WriteAsync(CmdStart, [startValue.Value]).ConfigureAwait(false);
        if (stopValue is not null)
            _ = await WriteAsync(CmdStop, [stopValue.Value]).ConfigureAwait(false);
        if (interval is not null)
            _ = await WriteAsync(CmdInterval, [interval.Value]).ConfigureAwait(false);
        if (length is not null)
            _ = await WriteAsync(CmdLength, [length.Value]).ConfigureAwait(false);
        if (edge is not null)
            _ = await WriteAsync(CmdEdge, [edge.Value]).ConfigureAwait(false);
        if (src is not null)
            _ = await WriteAsync(CmdSrc, [src.Value]).ConfigureAwait(false);
    }
}
