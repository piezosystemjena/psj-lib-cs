namespace PsjLib.Base.Capabilities;

/// <summary>
/// Write capability for target setpoint control.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Setpoint units match the device's position/control domain and are constrained by actuator and firmware limits.</para>
/// </remarks>
public class Setpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for writing setpoint value.
    /// </summary>
    internal const string CmdSetpoint = "SETPOINT";

    /// <summary>
    /// Writes a new setpoint value.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Device behavior on apply (including clamping and motion profile effects) is model-dependent.</para>
    /// </remarks>
    /// <param name="setpoint">Target setpoint in device units.</param>
    public virtual async Task SetAsync(double setpoint)
        => _ = await WriteAsync(CmdSetpoint, [setpoint]).ConfigureAwait(false);
}
