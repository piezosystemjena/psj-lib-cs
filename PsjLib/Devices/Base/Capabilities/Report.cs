namespace PsjLib.Base.Capabilities;

public sealed class Report(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdReport = "REPORT";
    public async Task<string> GetAsync() => (await WriteAsync(CmdReport).ConfigureAwait(false))[0];
}
