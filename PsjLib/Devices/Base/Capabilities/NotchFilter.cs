namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring notch filter behavior.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Effective notch tuning depends on resonance frequency selection and bandwidth tradeoffs between focused suppression and broader attenuation.</para>
/// </remarks>
public sealed class NotchFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for notch filter enable state.
    /// </summary>
    internal const string CmdEnable = "NOTCH_FILTER_ENABLE";
    /// <summary>
    /// Command token for notch center frequency.
    /// </summary>
    internal const string CmdFrequency = "NOTCH_FILTER_FREQUENCY";
    /// <summary>
    /// Command token for notch bandwidth.
    /// </summary>
    internal const string CmdBandwidth = "NOTCH_FILTER_BANDWIDTH";

    /// <summary>
    /// Updates notch filter settings.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null parameters are written, and valid frequency/bandwidth ranges are device-dependent.</para>
    /// </remarks>
    /// <param name="enabled">Optional enable state.</param>
    /// <param name="frequency">Optional center frequency.</param>
    /// <param name="bandwidth">Optional bandwidth.</param>
    public async Task SetAsync(bool? enabled = null, double? frequency = null, double? bandwidth = null)
    {
        if (enabled is not null)
            _ = await WriteAsync(CmdEnable, [enabled.Value]).ConfigureAwait(false);
        if (frequency is not null)
            _ = await WriteAsync(CmdFrequency, [frequency.Value]).ConfigureAwait(false);
        if (bandwidth is not null)
            _ = await WriteAsync(CmdBandwidth, [bandwidth.Value]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads notch filter enable state.
    /// </summary>
    /// <returns><see langword="true"/> when enabled.</returns>
    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
    /// <summary>
    /// Reads notch center frequency.
    /// </summary>
    /// <returns>Center frequency value.</returns>
    public async Task<double> GetFrequencyAsync() => double.Parse((await WriteAsync(CmdFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
    /// <summary>
    /// Reads notch bandwidth.
    /// </summary>
    /// <returns>Bandwidth value.</returns>
    public async Task<double> GetBandwidthAsync() => double.Parse((await WriteAsync(CmdBandwidth).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
