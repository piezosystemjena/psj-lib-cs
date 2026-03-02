namespace PsjLib.Base.Capabilities;

public sealed class SlewRate(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdRate = "SLEW_RATE";

    public async Task SetAsync(double rate)
        => _ = await WriteAsync(CmdRate, [rate]).ConfigureAwait(false);

    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdRate).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
