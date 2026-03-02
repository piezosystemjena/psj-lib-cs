namespace PsjLib.Base.Capabilities;

public sealed class FactoryReset(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdReset = "FACTORY_RESET";
    public async Task ExecuteAsync() => _ = await WriteAsync(CmdReset).ConfigureAwait(false);
}
