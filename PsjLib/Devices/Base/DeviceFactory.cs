using PsjLib.Transport;

namespace PsjLib.Base;

/// <summary>
/// Global registry mapping device identifiers to concrete C# constructors.
/// </summary>
internal static class DeviceModelRegistry
{
    /// <summary>
    /// Gets registered model constructors keyed by device identifier.
    /// </summary>
    internal static readonly Dictionary<string, Func<TransportType, string, PiezoDevice>> Registry = new(StringComparer.Ordinal);
}

/// <summary>
/// Creates strongly typed <see cref="PiezoDevice"/> instances from discovery information.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Factory resolution relies on <see cref="DeviceModelRegistry"/> entries, so supported device types must be registered before creation.</para>
/// </remarks>
internal static class DeviceFactory
{
    /// <summary>
    /// Creates a device instance for a detected endpoint.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> This overload is intended for discovery-driven flows that pass through <see cref="DetectedDevice"/> metadata.</para>
    /// </remarks>
    /// <param name="detectedDevice">Discovery result containing transport and model metadata.</param>
    /// <returns>New device instance for the detected model and endpoint.</returns>
    internal static PiezoDevice FromDetectedDevice(DetectedDevice detectedDevice)
    {
        if (detectedDevice.DeviceId is null)
        {
            throw new ArgumentException("Detected device has no device ID", nameof(detectedDevice));
        }

        return FromId(detectedDevice.DeviceId, detectedDevice.Transport, detectedDevice.Identifier);
    }

    /// <summary>
    /// Creates a device instance from an explicit model identifier and endpoint.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Device constructors are selected by registered model identifier; unknown IDs result in an <see cref="ArgumentException"/>.</para>
    /// </remarks>
    /// <param name="deviceId">Registered device model identifier.</param>
    /// <param name="transportType">Transport backend.</param>
    /// <param name="identifier">Transport endpoint identifier.</param>
    /// <returns>New device instance.</returns>
    internal static PiezoDevice FromId(string deviceId, TransportType transportType, string identifier)
    {
        if (!DeviceModelRegistry.Registry.TryGetValue(deviceId, out var ctor))
        {
            throw new ArgumentException($"Unsupported device ID: {deviceId}", nameof(deviceId));
        }

        return ctor(transportType, identifier);
    }
}
