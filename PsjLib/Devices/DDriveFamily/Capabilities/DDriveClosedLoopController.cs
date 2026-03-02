using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public sealed class DDriveClosedLoopController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : ClosedLoopController(writeCb, commands, samplePeriod)
{
    public const string CmdStatus = "STATUS";
    public override async Task<bool> GetEnabledAsync()
        => (int.Parse((await WriteAsync(CmdStatus).ConfigureAwait(false))[0]) & 0x80) != 0;
}
