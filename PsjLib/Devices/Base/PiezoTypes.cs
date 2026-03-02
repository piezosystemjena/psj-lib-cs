using PsjLib.Transport;

namespace PsjLib.Base;

public enum SensorType
{
    None = 0,
    StrainGauge = 1,
    Capacitive = 2,
    Inductive = 3,
    Unknown = 99,
}

public enum ActorType
{
    NanoX = 0,
    Psh = 1,
    Parallel = 2,
    Unknown = 99,
}

public sealed record DeviceInfo(TransportProtocolInfo TransportInfo, string? DeviceId = null, IReadOnlyDictionary<string, string>? ExtendedInfo = null);
