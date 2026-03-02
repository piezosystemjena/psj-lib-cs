namespace PsjLib.Base.Capabilities;

public sealed class InputVoltage(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdInputVoltage = "INPUT_VOLTAGE";
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdInputVoltage).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
