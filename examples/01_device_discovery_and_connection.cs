using PsjLib.Base;
using PsjLib.DDriveFamily;

namespace PsjLib.Examples;

public static class Example01DeviceDiscoveryAndConnection
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("Piezo device discovery and connection");
        Console.WriteLine(new string('=', 60));

        Console.WriteLine("\n[1] Discovering devices on all interfaces...");

        var discovered = await PiezoDevice.DiscoverDevicesAsync<PiezoDevice>(Transport.DiscoverFlags.DetectSerial).ConfigureAwait(false);

        if (discovered.Count == 0)
        {
            Console.WriteLine("\n❌ No devices found!");
            Console.WriteLine("\nTroubleshooting:");
            Console.WriteLine("  - Check device is powered on");
            Console.WriteLine("  - Verify USB/Serial cable is connected");
            Console.WriteLine("  - Check device is on same network (for Telnet)");
            Console.WriteLine("  - Ensure no other software has device open");
            return;
        }

        Console.WriteLine($"\n✓ Found {discovered.Count} device(s):\n");
        for (var i = 0; i < discovered.Count; i++)
        {
            var info = discovered[i].DeviceInfo;
            Console.WriteLine($"Device {i + 1}:");
            Console.WriteLine($"  Type:       {info.DeviceId}");
            Console.WriteLine($"  Transport:  {info.TransportInfo.Transport}");
            Console.WriteLine($"  Identifier: {info.TransportInfo.Identifier}\n");
        }

        Console.WriteLine("[2] Connecting to first d-Drive device...");
        var device = discovered.OfType<DDriveDevice>().FirstOrDefault();
        if (device is null)
        {
            Console.WriteLine("❌ No d-Drive device found!");
            return;
        }

        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Connected successfully!");

        Console.WriteLine("\n[3] Connected device Information:");
        var connectedInfo = device.DeviceInfo;
        Console.WriteLine($"  Device Type: {connectedInfo.DeviceId}");
        Console.WriteLine($"  Connection:  {connectedInfo.TransportInfo.Transport} on {connectedInfo.TransportInfo.Identifier}");

        Console.WriteLine("\n[4] Available Channels:");
        if (device.Channels.Count == 0)
        {
            Console.WriteLine("  No channels found!");
        }
        else
        {
            Console.WriteLine($"  Total channels: {device.Channels.Count}");
            foreach (var channel in device.Channels)
            {
                Console.WriteLine($"    - Channel {channel.Key}");
            }
        }

        foreach (var (channelId, channel) in device.Channels)
        {
            Console.WriteLine($"\n[5] Querying Channel {channelId} Information:");
            try
            {
                var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
                Console.WriteLine($"  Temperature: {temp:F1}°C");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Temperature: (error - {ex.Message})");
            }

            try
            {
                var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
                Console.WriteLine("  Status:");
                Console.WriteLine($"    - Actuator plugged: {status.ActorPlugged}");
                Console.WriteLine($"    - Sensor type: {status.SensorType}");
                Console.WriteLine($"    - Closed-loop: {status.ClosedLoop}");
                Console.WriteLine($"    - Voltage enabled: {status.PiezoVoltageEnabled}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Status: (error - {ex.Message})");
            }

            Console.WriteLine();
        }

        Console.WriteLine("[6] Disconnecting...");
        await device.CloseAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Disconnected successfully!");

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Example completed!");
        Console.WriteLine(new string('=', 60));
    }
}
