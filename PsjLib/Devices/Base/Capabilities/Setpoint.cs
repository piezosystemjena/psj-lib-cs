namespace PsjLib.Base.Capabilities;

public class Setpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdSetpoint = "SETPOINT";

    public virtual async Task SetAsync(double setpoint)
        => _ = await WriteAsync(CmdSetpoint, [setpoint]).ConfigureAwait(false);
}
