namespace PsjLib.Transport;

public static class TransportFactory
{
    public static TransportProtocol FromDetectedDevice(DetectedDevice detectedDevice)
    {
        return FromTransportType(detectedDevice.Transport, detectedDevice.Identifier);
    }

    public static TransportProtocol FromTransportType(TransportType transportType, string identifier)
    {
        return transportType switch
        {
            TransportType.Serial => new SerialProtocol(identifier),
            TransportType.Telnet => new TelnetProtocol(identifier),
            _ => throw new ArgumentOutOfRangeException(nameof(transportType), transportType, "Unsupported transport type"),
        };
    }
}
