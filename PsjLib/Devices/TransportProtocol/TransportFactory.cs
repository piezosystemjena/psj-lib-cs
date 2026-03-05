namespace PsjLib.Transport;

/// <summary>
/// Factory helpers for transport protocol instantiation.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Returned transports are unconnected; callers must invoke <c>ConnectAsync</c> before issuing commands.</para>
/// </remarks>
internal static class TransportFactory
{
    /// <summary>
    /// Creates a transport instance from a discovered endpoint.
    /// </summary>
    /// <param name="detectedDevice">Detected endpoint metadata.</param>
    /// <returns>Transport implementation matching the endpoint type.</returns>
    internal static TransportProtocol FromDetectedDevice(DetectedDevice detectedDevice)
    {
        return FromTransportType(detectedDevice.Transport, detectedDevice.Identifier);
    }

    /// <summary>
    /// Creates a transport instance from explicit type and identifier.
    /// </summary>
    /// <param name="transportType">Transport backend type.</param>
    /// <param name="identifier">Endpoint identifier (host, MAC, COM port, etc.).</param>
    /// <returns>Transport implementation.</returns>
    internal static TransportProtocol FromTransportType(TransportType transportType, string identifier)
    {
        return transportType switch
        {
            TransportType.Serial => new SerialProtocol(identifier),
            TransportType.Telnet => new TelnetProtocol(identifier),
            _ => throw new ArgumentOutOfRangeException(nameof(transportType), transportType, "Unsupported transport type"),
        };
    }
}
