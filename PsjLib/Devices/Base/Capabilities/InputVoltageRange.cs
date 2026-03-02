namespace PsjLib.Base.Capabilities;

public sealed class InputVoltageRange(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdInputVoltageRange = "INPUT_VOLTAGE_RANGE";
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdInputVoltageRange).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
