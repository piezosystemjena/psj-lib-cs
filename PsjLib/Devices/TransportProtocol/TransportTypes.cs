namespace PsjLib.Transport;

/// <summary>
/// Transport backends supported by this library.
/// </summary>
public enum TransportType
{
    /// <summary>
    /// Ethernet/Telnet based communication.
    /// </summary>
    Telnet,
    /// <summary>
    /// Serial (COM/USB virtual COM) communication.
    /// </summary>
    Serial,
}

/// <summary>
/// Base exception for transport-layer failures.
/// </summary>
public class ProtocolException : Exception
{
    /// <summary>
    /// Initializes a new protocol exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    public ProtocolException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new protocol exception with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="inner">Underlying exception that caused this error.</param>
    public ProtocolException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Indicates that a target device is not reachable or no longer available.
/// </summary>
public sealed class DeviceUnavailableException : ProtocolException
{
    /// <summary>
    /// Initializes a new <see cref="DeviceUnavailableException"/>.
    /// </summary>
    /// <param name="message">Error message.</param>
    public DeviceUnavailableException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new <see cref="DeviceUnavailableException"/> with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="inner">Underlying exception.</param>
    public DeviceUnavailableException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Indicates that a transport read/write operation timed out.
/// </summary>
public sealed class TimeoutException : ProtocolException
{
    /// <summary>
    /// Initializes a new <see cref="TimeoutException"/>.
    /// </summary>
    /// <param name="message">Error message.</param>
    public TimeoutException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new <see cref="TimeoutException"/> with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="inner">Underlying exception.</param>
    public TimeoutException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Describes a configured transport endpoint.
/// </summary>
/// <param name="Transport">Transport backend type.</param>
/// <param name="Identifier">Primary endpoint identifier (COM port, host, etc.).</param>
/// <param name="Mac">Optional MAC address (network transports).</param>
public sealed record TransportProtocolInfo(TransportType Transport, string Identifier, string? Mac = null)
{
    /// <summary>
    /// Returns a concise human-readable transport description.
    /// </summary>
    /// <returns>Formatted transport description.</returns>
    public override string ToString() => $"{Transport} @ {Identifier}";
}

/// <summary>
/// Represents a discovered endpoint, optionally including identified model metadata.
/// </summary>
/// <param name="Transport">Transport backend where this endpoint was found.</param>
/// <param name="Identifier">Endpoint identifier used for reconnection.</param>
/// <param name="Mac">Optional MAC address for network devices.</param>
/// <param name="DeviceId">Optional model identifier detected by probing.</param>
public sealed record DetectedDevice(TransportType Transport, string Identifier, string? Mac = null, string? DeviceId = null)
{
    /// <summary>
    /// Returns a formatted discovery description containing identifier and optional metadata.
    /// </summary>
    /// <returns>Human-readable discovery string.</returns>
    public override string ToString()
    {
        var result = $"{Transport} @ {Identifier}";
        if (!string.IsNullOrWhiteSpace(Mac))
        {
            result += $" (MAC: {Mac})";
        }

        if (!string.IsNullOrWhiteSpace(DeviceId))
        {
            result += $" - {DeviceId}";
        }

        return result;
    }
}

/// <summary>
/// Bit flags controlling discovery scope and behavior.
/// </summary>
[Flags]
public enum DiscoverFlags
{
    /// <summary>
    /// Enable serial device discovery.
    /// </summary>
    DetectSerial = 1,
    /// <summary>
    /// Enable Ethernet/Telnet device discovery.
    /// </summary>
    DetectEthernet = 2,
    /// <summary>
    /// Allow transport-specific communication parameter adjustments during connect/discovery.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> This primarily affects network/Telnet workflows (for example Lantronix communication tuning) and can increase discovery/connect time.</para>
    /// </remarks>
    AdjustCommParams = 4,
    /// <summary>
    /// Discover on all supported interfaces, without communication parameter adjustment.
    /// </summary>
    AllInterfaces = DetectSerial | DetectEthernet,
    /// <summary>
    /// Full discovery including all interfaces and optional communication parameter adjustment.
    /// </summary>
    All = AllInterfaces | AdjustCommParams,

}
