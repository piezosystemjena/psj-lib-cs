namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring and reading slew rate limiting.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Slew-rate units and effective range are device-specific; lower values smooth transitions while increasing response time.</para>
/// </remarks>
public sealed class SlewRate(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for slew rate.
    /// </summary>
    public const string CmdRate = "SLEW_RATE";

    /// <summary>
    /// Sets slew rate.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Valid values are hardware-dependent, and extreme values can effectively remove or strongly enforce rate limiting.</para>
    /// </remarks>
    /// <param name="rate">Slew rate value in device units.</param>
    public async Task SetAsync(double rate)
        => _ = await WriteAsync(CmdRate, [rate]).ConfigureAwait(false);

    /// <summary>
    /// Reads current slew rate.
    /// </summary>
    /// <returns>Slew rate value.</returns>
    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdRate).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
