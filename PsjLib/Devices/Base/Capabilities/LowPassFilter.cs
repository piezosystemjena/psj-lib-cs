namespace PsjLib.Base.Capabilities;

public sealed class LowPassFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdEnable = "LOW_PASS_FILTER_ENABLE";
    public const string CmdCutoffFrequency = "LOW_PASS_FILTER_CUTOFF_FREQUENCY";

    public async Task SetAsync(bool? enabled = null, double? cutoffFrequency = null)
    {
        if (enabled is not null)
            _ = await WriteAsync(CmdEnable, [enabled.Value]).ConfigureAwait(false);
        if (cutoffFrequency is not null)
            _ = await WriteAsync(CmdCutoffFrequency, [cutoffFrequency.Value]).ConfigureAwait(false);
    }

    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
    public async Task<double> GetCutoffFrequencyAsync() => double.Parse((await WriteAsync(CmdCutoffFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
