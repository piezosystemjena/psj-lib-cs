namespace PsjLib.Transport;

public static class DeviceDiscovery
{
    public static async Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(
        DiscoveryCallback discoveryCallback,
        DiscoverFlags flags = DiscoverFlags.AllInterfaces)
    {
        var tasks = new List<Task<IReadOnlyList<DetectedDevice>>>();

        if (flags.HasFlag(DiscoverFlags.DetectSerial))
        {
            tasks.Add(new SerialProtocol("COM0").DiscoverDevicesAsync(discoveryCallback));
        }

        if (flags.HasFlag(DiscoverFlags.DetectEthernet))
        {
            tasks.Add(new TelnetProtocol().DiscoverDevicesAsync(discoveryCallback));
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.SelectMany(static x => x).ToList();
    }

    public static DiscoverFlags FlagsForTransport(TransportType? transportType = null)
    {
        if (transportType is null)
        {
            return DiscoverFlags.AllInterfaces;
        }

        return transportType.Value switch
        {
            TransportType.Serial => DiscoverFlags.DetectSerial,
            TransportType.Telnet => DiscoverFlags.DetectEthernet,
            _ => throw new ArgumentOutOfRangeException(nameof(transportType), transportType, "Unsupported transport type"),
        };
    }
}
