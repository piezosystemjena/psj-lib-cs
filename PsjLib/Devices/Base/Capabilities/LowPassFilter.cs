namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring output low-pass filtering.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Lower cutoff values increase filtering but slow response, while higher cutoff values reduce filtering and phase lag.</para>
/// </remarks>
public sealed class LowPassFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for low-pass filter enable state.
    /// </summary>
    public const string CmdEnable = "LOW_PASS_FILTER_ENABLE";
    /// <summary>
    /// Command token for low-pass filter cutoff frequency.
    /// </summary>
    public const string CmdCutoffFrequency = "LOW_PASS_FILTER_CUTOFF_FREQUENCY";

    /// <summary>
    /// Updates low-pass filter settings.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null parameters are written; valid cutoff range is device-dependent.</para>
    /// </remarks>
    /// <param name="enabled">Optional filter enable state.</param>
    /// <param name="cutoffFrequency">Optional cutoff frequency.</param>
    public async Task SetAsync(bool? enabled = null, double? cutoffFrequency = null)
    {
        if (enabled is not null)
            _ = await WriteAsync(CmdEnable, [enabled.Value]).ConfigureAwait(false);
        if (cutoffFrequency is not null)
            _ = await WriteAsync(CmdCutoffFrequency, [cutoffFrequency.Value]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads low-pass filter enable state.
    /// </summary>
    /// <returns><see langword="true"/> when filter is enabled.</returns>
    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
    /// <summary>
    /// Reads low-pass filter cutoff frequency.
    /// </summary>
    /// <returns>Cutoff frequency value.</returns>
    public async Task<double> GetCutoffFrequencyAsync() => double.Parse((await WriteAsync(CmdCutoffFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
