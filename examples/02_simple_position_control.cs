using PsjLib.DDriveFamily;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example02SimplePositionControl
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive Simple Position Control");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM6");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine($"✓ Connected to device with {device.Channels.Count} channel(s)\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;

        try
        {
            Console.WriteLine("[1] Reading device status...");
            var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"  Actuator connected: {status.ActorPlugged}");
            Console.WriteLine($"  Sensor type: {status.SensorType}");
            Console.WriteLine($"  Closed-loop active: {status.ClosedLoop}\n");

            Console.WriteLine("[2] Testing Open-Loop Control...");
            await channel.ClosedLoopController.SetAsync(false).ConfigureAwait(false);
            Console.WriteLine("  ✓ Closed-loop disabled (open-loop mode)");

            await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);

            var setpoint = await channel.Setpoint.GetAsync().ConfigureAwait(false);
            var position = await channel.Position.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"  Setpoint: {setpoint:F2} V");
            Console.WriteLine($"  Position: {position:F2} V\n");

            Console.WriteLine("[3] Testing Closed-Loop Control...");
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
            Console.WriteLine("  ✓ Closed-loop enabled (feedback control active)");

            var targetPositions = new[] { 30.0, 50.0, 70.0, 50.0 };
            foreach (var target in targetPositions)
            {
                Console.WriteLine($"\n  Moving to {target:F1} µm...");
                await channel.Setpoint.SetAsync(target).ConfigureAwait(false);
                await Task.Delay(1000).ConfigureAwait(false);

                var actual = await channel.Position.GetAsync().ConfigureAwait(false);
                var error = Math.Abs(target - actual);

                Console.WriteLine($"    Target: {target:F2} µm");
                Console.WriteLine($"    Actual: {actual:F2} µm");
                Console.WriteLine($"    Error:  {error:F2} µm");
                Console.WriteLine(error < 0.5 ? "    ✓ Position achieved!" : "    ⚠ Large position error");
            }

            Console.WriteLine("\n[4] Final Status...");
            var finalPos = await channel.Position.GetAsync().ConfigureAwait(false);
            var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"  Position: {finalPos:F2} µm");
            Console.WriteLine($"  Temperature: {temp:F1}°C");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("\n✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Position Control Summary:");
        Console.WriteLine("- Open-loop: Direct voltage control, no feedback");
        Console.WriteLine("- Closed-loop: Sensor feedback for precise positioning");
        Console.WriteLine("- Always use closed-loop for accurate position control");
        Console.WriteLine("- Allow settling time after position changes");
        Console.WriteLine(new string('=', 60));
    }
}
