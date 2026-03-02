namespace PsjLib.Base.Capabilities;

public sealed class Status<TStatusRegister>(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
    where TStatusRegister : StatusRegister
{
    public const string CmdStatus = "STATUS";

    public async Task<TStatusRegister> GetAsync()
    {
        var raw = await WriteAsync(CmdStatus).ConfigureAwait(false);
        return (TStatusRegister)Activator.CreateInstance(typeof(TStatusRegister), raw)!;
    }
}
