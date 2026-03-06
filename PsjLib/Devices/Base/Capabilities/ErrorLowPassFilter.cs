namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring low-pass filtering of control error.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Error filtering affects closed-loop behavior only; higher order or lower cutoff increases noise suppression but can add phase lag and slow response.</para>
/// </remarks>
public sealed class ErrorLowPassFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for error low-pass filter cutoff frequency.
    /// </summary>
    internal const string CmdCutoffFrequency = "ERROR_LOW_PASS_FILTER_CUTOFF_FREQUENCY";
    /// <summary>
    /// Command token for error low-pass filter order.
    /// </summary>
    internal const string CmdOrder = "ERROR_LOW_PASS_FILTER_ORDER";

    /// <summary>
    /// Updates error low-pass filter settings.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null parameters are written, and filter order/cutoff should be coordinated with controller tuning to preserve stability margins.</para>
    /// </remarks>
    /// <param name="cutoffFrequency">Optional cutoff frequency value.</param>
    /// <param name="order">Optional filter order.</param>
    public async Task SetAsync(double? cutoffFrequency = null, int? order = null)
    {
        if (cutoffFrequency is not null)
            _ = await WriteAsync(CmdCutoffFrequency, [cutoffFrequency.Value]).ConfigureAwait(false);
        if (order is not null)
            _ = await WriteAsync(CmdOrder, [order.Value]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads current error low-pass filter cutoff frequency.
    /// </summary>
    /// <returns>Cutoff frequency value.</returns>
    public async Task<double> GetCutoffFrequencyAsync()
        => double.Parse((await WriteAsync(CmdCutoffFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads current error low-pass filter order.
    /// </summary>
    /// <returns>Filter order.</returns>
    public async Task<int> GetOrderAsync()
        => int.Parse((await WriteAsync(CmdOrder).ConfigureAwait(false))[0]);
}
