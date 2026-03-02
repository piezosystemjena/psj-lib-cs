namespace PsjLib.Base.Capabilities;

public sealed class ErrorLowPassFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdCutoffFrequency = "ERROR_LOW_PASS_FILTER_CUTOFF_FREQUENCY";
    public const string CmdOrder = "ERROR_LOW_PASS_FILTER_ORDER";

    public async Task SetAsync(double? cutoffFrequency = null, int? order = null)
    {
        if (cutoffFrequency is not null)
            _ = await WriteAsync(CmdCutoffFrequency, [cutoffFrequency.Value]).ConfigureAwait(false);
        if (order is not null)
            _ = await WriteAsync(CmdOrder, [order.Value]).ConfigureAwait(false);
    }

    public async Task<double> GetCutoffFrequencyAsync()
        => double.Parse((await WriteAsync(CmdCutoffFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    public async Task<int> GetOrderAsync()
        => int.Parse((await WriteAsync(CmdOrder).ConfigureAwait(false))[0]);
}
