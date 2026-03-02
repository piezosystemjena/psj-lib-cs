namespace PsjLib.Base.Capabilities;

public sealed class PreControlFactor(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdValue = "PRE_CONTROL_FACTOR";

    public async Task SetAsync(double value)
        => _ = await WriteAsync(CmdValue, [value]).ConfigureAwait(false);

    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdValue).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
