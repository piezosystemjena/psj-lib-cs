namespace PsjLib.Base.Capabilities;

public sealed class NotchFilter(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdEnable = "NOTCH_FILTER_ENABLE";
    public const string CmdFrequency = "NOTCH_FILTER_FREQUENCY";
    public const string CmdBandwidth = "NOTCH_FILTER_BANDWIDTH";

    public async Task SetAsync(bool? enabled = null, double? frequency = null, double? bandwidth = null)
    {
        if (enabled is not null)
            _ = await WriteAsync(CmdEnable, [enabled.Value]).ConfigureAwait(false);
        if (frequency is not null)
            _ = await WriteAsync(CmdFrequency, [frequency.Value]).ConfigureAwait(false);
        if (bandwidth is not null)
            _ = await WriteAsync(CmdBandwidth, [bandwidth.Value]).ConfigureAwait(false);
    }

    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
    public async Task<double> GetFrequencyAsync() => double.Parse((await WriteAsync(CmdFrequency).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
    public async Task<double> GetBandwidthAsync() => double.Parse((await WriteAsync(CmdBandwidth).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
