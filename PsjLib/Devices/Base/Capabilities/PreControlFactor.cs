namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for pre-control/feed-forward factor tuning.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Pre-control/feed-forward tuning ranges and effects are device-specific; higher values can improve response speed but may increase overshoot risk.</para>
/// </remarks>
public sealed class PreControlFactor(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for pre-control factor value.
    /// </summary>
    internal const string CmdValue = "PRE_CONTROL_FACTOR";

    /// <summary>
    /// Sets pre-control factor value.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Valid limits depend on firmware/hardware and should be tuned alongside closed-loop controller gains.</para>
    /// </remarks>
    /// <param name="value">Pre-control factor.</param>
    public async Task SetAsync(double value)
        => _ = await WriteAsync(CmdValue, [value]).ConfigureAwait(false);

    /// <summary>
    /// Reads pre-control factor value.
    /// </summary>
    /// <returns>Current pre-control factor.</returns>
    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdValue).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
