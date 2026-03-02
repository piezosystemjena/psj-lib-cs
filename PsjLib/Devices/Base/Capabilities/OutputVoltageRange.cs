namespace PsjLib.Base.Capabilities;

public sealed class OutputVoltageRange(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdOutputVoltageRange = "OUTPUT_VOLTAGE_RANGE";
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdOutputVoltageRange).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
