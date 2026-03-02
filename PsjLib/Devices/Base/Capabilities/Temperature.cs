namespace PsjLib.Base.Capabilities;

public sealed class Temperature(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdTemperature = "TEMPERATURE";

    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdTemperature).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
