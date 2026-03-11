namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for enabling/disabling closed-loop control.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Closed-loop operation depends on valid sensor feedback and properly tuned controller parameters; sample period is provided by the owning device family.</para>
/// </remarks>
public class ClosedLoopController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for closed-loop enable state.
    /// </summary>
    internal const string CmdEnable = "CLOSED_LOOP_CONTROLLER_ENABLE";

    /// <summary>
    /// Gets controller sample period in microseconds.
    /// </summary>
    public int SamplePeriod { get; } = samplePeriod;
    /// <summary>
    /// Gets controller sample rate in hertz.
    /// </summary>
    public double SampleRate => SamplePeriod > 0 ? 1000000.0 / SamplePeriod : double.NaN;

    /// <summary>
    /// Enables or disables closed-loop control.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable closed-loop mode.</param>
    public virtual async Task SetAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);

    /// <summary>
    /// Reads closed-loop enable state.
    /// </summary>
    /// <returns><see langword="true"/> when closed-loop mode is active.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Base implementations typically read a dedicated command; some derived device families override this to read status-register bits.</para>
    /// </remarks>
    public virtual async Task<bool> GetEnabledAsync()
    {
        var result = await WriteAsync(CmdEnable).ConfigureAwait(false);

        // If the device returns no data, assume closed-loop is not enabled.
        // This an happen if the actuator is not plugged in.
        if (result is null || result.Count == 0)
        {
            return false;
        }

        return int.Parse(result[0]) != 0;  
    } 
}
