using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public sealed class DDriveTriggerOut(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : TriggerOut(writeCb, commands)
{
    public const string CmdOffset = "TRIGGER_OUT_OFFSET";

    public async Task SetAsync(
        double? startValue = null,
        double? stopValue = null,
        double? interval = null,
        int? length = null,
        TriggerEdge? edge = null,
        TriggerDataSource? src = null,
        double? offset = null)
    {
        await base.SetAsync(startValue, stopValue, interval, length, edge, src).ConfigureAwait(false);
        if (offset is not null)
        {
            _ = await WriteAsync(CmdOffset, [offset.Value]).ConfigureAwait(false);
        }
    }

    public async Task<double> GetOffsetAsync() => double.Parse((await WriteAsync(CmdOffset).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
