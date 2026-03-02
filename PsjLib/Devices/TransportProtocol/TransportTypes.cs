namespace PsjLib.Transport;

public enum TransportType
{
    Telnet,
    Serial,
}

public class ProtocolException : Exception
{
    public ProtocolException(string message) : base(message) { }
    public ProtocolException(string message, Exception inner) : base(message, inner) { }
}

public sealed class DeviceUnavailableException : ProtocolException
{
    public DeviceUnavailableException(string message) : base(message) { }
    public DeviceUnavailableException(string message, Exception inner) : base(message, inner) { }
}

public sealed class TimeoutException : ProtocolException
{
    public TimeoutException(string message) : base(message) { }
    public TimeoutException(string message, Exception inner) : base(message, inner) { }
}

public sealed record TransportProtocolInfo(TransportType Transport, string Identifier, string? Mac = null)
{
    public override string ToString() => $"{Transport} @ {Identifier}";
}

public sealed record DetectedDevice(TransportType Transport, string Identifier, string? Mac = null, string? DeviceId = null)
{
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

[Flags]
public enum DiscoverFlags
{
    DetectSerial = 1,
    DetectEthernet = 2,
    AdjustCommParams = 4,
    AllInterfaces = DetectSerial | DetectEthernet,
    All = AllInterfaces | AdjustCommParams,

}
