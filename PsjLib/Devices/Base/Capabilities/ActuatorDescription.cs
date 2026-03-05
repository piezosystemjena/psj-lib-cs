namespace PsjLib.Base.Capabilities;

/// <summary>
/// Provides access to actuator description text reported by the device.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Description text is actuator-specific and may include model, travel range, and resolution details; some devices can return an empty string when description data is unavailable.</para>
/// </remarks>
public sealed class ActuatorDescription(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for actuator description query.
    /// </summary>
    internal const string CmdDescription = "actuator_description";

    /// <summary>
    /// Reads the actuator description string from the device.
    /// </summary>
    /// <returns>Actuator description text.</returns>
    public async Task<string> GetAsync() => (await WriteAsync(CmdDescription).ConfigureAwait(false))[0];
}
