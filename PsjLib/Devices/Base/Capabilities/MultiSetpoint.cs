namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for synchronous multi-channel setpoint writes.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Some devices require additional preconditions (for example actuator presence and modulation mode) for coordinated writes to be accepted.</para>
/// </remarks>
public class MultiSetpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int channelCount)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for multi-setpoint write.
    /// </summary>
    internal const string CmdSetpoints = "SETPOINTS";
    private readonly int _channelCount = channelCount;

    /// <summary>
    /// Writes setpoints for all channels in a single command.
    /// </summary>
    /// <param name="setpoints">Setpoint values for each channel.</param>
    /// <exception cref="ArgumentException">Raised when list length does not match configured channel count.</exception>
    public async Task SetAsync(IReadOnlyList<double> setpoints)
    {
        if (setpoints.Count != _channelCount)
        {
            throw new ArgumentException($"Expected {_channelCount} setpoints, got {setpoints.Count}", nameof(setpoints));
        }

        _ = await WriteAsync(CmdSetpoints, setpoints.Cast<object?>().ToList()).ConfigureAwait(false);
    }
}
