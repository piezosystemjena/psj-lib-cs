namespace PsjLib.Base.Capabilities;

public sealed class Position(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdPosition = "POSITION";

    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdPosition).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
