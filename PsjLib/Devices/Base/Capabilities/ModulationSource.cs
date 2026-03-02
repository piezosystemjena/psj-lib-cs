namespace PsjLib.Base.Capabilities;

public sealed class ModulationSource(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, Type enumType)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdSource = "MODULATION_SOURCE";
    private readonly Type _enumType = enumType;

    public async Task SetAsync(Enum source)
        => _ = await WriteAsync(CmdSource, [source]).ConfigureAwait(false);

    public async Task<Enum> GetAsync()
    {
        var value = int.Parse((await WriteAsync(CmdSource).ConfigureAwait(false))[0]);
        return Enum.IsDefined(_enumType, value)
            ? (Enum)Enum.ToObject(_enumType, value)
            : (Enum)Enum.ToObject(_enumType, 0);
    }
}
