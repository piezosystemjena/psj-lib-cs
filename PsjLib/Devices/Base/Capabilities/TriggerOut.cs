namespace PsjLib.Base.Capabilities;

/// <summary>
/// Trigger polarity for trigger-out signal generation.
/// </summary>
public enum TriggerEdge
{
    /// <summary>
    /// Pulse transitions low-to-high at trigger event.
    /// </summary>
    Rising = 0,
    /// <summary>
    /// Pulse transitions high-to-low at trigger event.
    /// </summary>
    Falling = 1,
}

/// <summary>
/// Data source used for trigger thresholding.
/// </summary>
public enum TriggerDataSource
{
    /// <summary>
    /// Trigger source is measured position.
    /// </summary>
    Position = 0,
    /// <summary>
    /// Trigger source is output voltage.
    /// </summary>
    Voltage = 1,
}

/// <summary>
/// Capability for configuring trigger-out generation.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Trigger behavior is device-dependent (threshold units, edge behavior, and timing base). Parameters are optional and only provided values are updated.</para>
/// </remarks>
public class TriggerOut(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for trigger start threshold.
    /// </summary>
    public const string CmdStart = "TRIGGER_OUT_START";
    /// <summary>
    /// Command token for trigger stop threshold.
    /// </summary>
    public const string CmdStop = "TRIGGER_OUT_STOP";
    /// <summary>
    /// Command token for trigger interval.
    /// </summary>
    public const string CmdInterval = "TRIGGER_OUT_INTERVAL";
    /// <summary>
    /// Command token for trigger pulse length.
    /// </summary>
    public const string CmdLength = "TRIGGER_OUT_LENGTH";
    /// <summary>
    /// Command token for trigger polarity.
    /// </summary>
    public const string CmdEdge = "TRIGGER_OUT_EDGE";
    /// <summary>
    /// Command token for trigger source selection.
    /// </summary>
    public const string CmdSrc = "TRIGGER_OUT_SOURCE";

    /// <summary>
    /// Updates one or more trigger output parameters.
    /// </summary>
    /// <param name="startValue">Optional start threshold.</param>
    /// <param name="stopValue">Optional stop threshold.</param>
    /// <param name="interval">Optional trigger interval.</param>
    /// <param name="length">Optional trigger pulse length.</param>
    /// <param name="edge">Optional trigger edge polarity.</param>
    /// <param name="src">Optional trigger source.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Leaving a parameter as <see langword="null"/> keeps the current device value unchanged.</para>
    /// </remarks>
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
