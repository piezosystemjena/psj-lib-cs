namespace PsjLib.Base.Capabilities;

public sealed class ServoRate(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdServoRate = "SERVO_RATE";
    public async Task SetAsync(int servoRate) => _ = await WriteAsync(CmdServoRate, [servoRate]).ConfigureAwait(false);
    public async Task<int> GetAsync() => int.Parse((await WriteAsync(CmdServoRate).ConfigureAwait(false))[0]);
}
