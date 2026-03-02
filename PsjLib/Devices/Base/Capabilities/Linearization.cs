namespace PsjLib.Base.Capabilities;

public sealed class Linearization(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdEnable = "LINEARIZATION_ENABLE";
    public async Task SetAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);
    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
