namespace PsjLib.Transport;

/// <summary>
/// Multi-transport discovery utilities for piezo controllers.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Discovery latency depends on enabled transports and network conditions; serial and ethernet probes are executed concurrently when both are enabled.</para>
/// </remarks>
public static class DeviceDiscovery
{
    /// <summary>
    /// Discovers devices on selected transports and probes each candidate using the supplied callback.
    /// </summary>
    /// <param name="discoveryCallback">Probe callback that returns a model identifier for recognized devices.</param>
    /// <param name="flags">Discovery behavior flags.</param>
    /// <returns>Flattened list of all discovered and recognized endpoints.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Empty results are valid when no matching devices are reachable on the selected interfaces.</para>
    /// </remarks>
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

    /// <summary>
    /// Converts a specific transport type to matching discovery flags.
    /// </summary>
    /// <param name="transportType">Transport type, or <see langword="null"/> for all interfaces.</param>
    /// <returns>Discovery flags for the requested transport scope.</returns>
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
