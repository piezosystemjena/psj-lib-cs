using PsjLib.Transport;

namespace PsjLib.Base;

/// <summary>
/// Sensor technologies supported by piezo devices in this library.
/// </summary>
public enum SensorType
{
    /// <summary>
    /// Device reports no sensor.
    /// </summary>
    None = 0,
    /// <summary>
    /// Strain gauge feedback sensor.
    /// </summary>
    StrainGauge = 1,
    /// <summary>
    /// Capacitive feedback sensor.
    /// </summary>
    Capacitive = 2,
    /// <summary>
    /// Inductive feedback sensor.
    /// </summary>
    Inductive = 3,
    /// <summary>
    /// Sensor type could not be mapped to a known value.
    /// </summary>
    Unknown = 99,
}

/// <summary>
/// Actuator families that may be connected to a controller channel.
/// </summary>
public enum ActorType
{
    /// <summary>
    /// NanoX actuator.
    /// </summary>
    NanoX = 0,
    /// <summary>
    /// PSH actuator.
    /// </summary>
    Psh = 1,
    /// <summary>
    /// Parallel actuator configuration.
    /// </summary>
    Parallel = 2,
    /// <summary>
    /// Actuator type is unknown or unsupported.
    /// </summary>
    Unknown = 99,
}

/// <summary>
/// Snapshot of high-level device metadata.
/// </summary>
/// <param name="TransportInfo">Transport connection metadata used to communicate with the device.</param>
/// <param name="DeviceId">Reported device model identifier, if known.</param>
/// <param name="ExtendedInfo">Optional device-specific metadata returned by some models.</param>
public sealed record DeviceInfo(TransportProtocolInfo TransportInfo, string? DeviceId = null, IReadOnlyDictionary<string, string>? ExtendedInfo = null);
