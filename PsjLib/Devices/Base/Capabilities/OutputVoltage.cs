namespace PsjLib.Base.Capabilities;

public sealed class OutputVoltage(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdOutputVoltage = "OUTPUT_VOLTAGE";
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdOutputVoltage).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
