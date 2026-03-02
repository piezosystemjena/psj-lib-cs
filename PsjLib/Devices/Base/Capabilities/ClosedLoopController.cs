namespace PsjLib.Base.Capabilities;

public class ClosedLoopController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdEnable = "CLOSED_LOOP_CONTROLLER_ENABLE";
    public int SamplePeriod { get; } = samplePeriod;
    public double SampleRate => 1000000.0 / SamplePeriod;

    public virtual async Task SetAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);
    public virtual async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
