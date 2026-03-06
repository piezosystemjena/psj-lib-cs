namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring servo loop rate.
/// </summary>
public sealed class ServoRate(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for servo rate.
    /// </summary>
    internal const string CmdServoRate = "SERVO_RATE";

    /// <summary>
    /// Sets servo rate.
    /// </summary>
    /// <param name="servoRate">Servo rate value in firmware-specific units.</param>
    public async Task SetAsync(int servoRate) => _ = await WriteAsync(CmdServoRate, [servoRate]).ConfigureAwait(false);

    /// <summary>
    /// Reads configured servo rate.
    /// </summary>
    /// <returns>Servo rate value.</returns>
    public async Task<int> GetAsync() => int.Parse((await WriteAsync(CmdServoRate).ConfigureAwait(false))[0]);
}
