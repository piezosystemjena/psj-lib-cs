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
    public const string CmdEnable = "CLOSED_LOOP_CONTROLLER_ENABLE";

    /// <summary>
    /// Gets controller sample period in microseconds.
    /// </summary>
    public int SamplePeriod { get; } = samplePeriod;
    /// <summary>
    /// Gets controller sample rate in hertz.
    /// </summary>
    public double SampleRate => 1000000.0 / SamplePeriod;

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
    public virtual async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
