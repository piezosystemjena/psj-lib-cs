namespace PsjLib.Base.Capabilities;

public sealed class MonitorOutput(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, Type enumType)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdOutputSrc = "MONITOR_OUTPUT_SOURCE";
    private readonly Type _enumType = enumType;

    public async Task SetAsync(Enum source)
        => _ = await WriteAsync(CmdOutputSrc, [source]).ConfigureAwait(false);

    public async Task<Enum> GetAsync()
    {
        var value = int.Parse((await WriteAsync(CmdOutputSrc).ConfigureAwait(false))[0]);
        return Enum.IsDefined(_enumType, value)
            ? (Enum)Enum.ToObject(_enumType, value)
            : (Enum)Enum.ToObject(_enumType, 0);
    }
}
