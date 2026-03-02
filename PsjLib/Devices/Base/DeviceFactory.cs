using PsjLib.Transport;

namespace PsjLib.Base;

public static class DeviceModelRegistry
{
    public static readonly Dictionary<string, Func<TransportType, string, PiezoDevice>> Registry = new(StringComparer.Ordinal);
}

public static class DeviceFactory
{
    public static PiezoDevice FromDetectedDevice(DetectedDevice detectedDevice)
    {
        if (detectedDevice.DeviceId is null)
        {
            throw new ArgumentException("Detected device has no device ID", nameof(detectedDevice));
        }

        return FromId(detectedDevice.DeviceId, detectedDevice.Transport, detectedDevice.Identifier);
    }

    public static PiezoDevice FromId(string deviceId, TransportType transportType, string identifier)
    {
        if (!DeviceModelRegistry.Registry.TryGetValue(deviceId, out var ctor))
        {
            throw new ArgumentException($"Unsupported device ID: {deviceId}", nameof(deviceId));
        }

        return ctor(transportType, identifier);
    }
}
