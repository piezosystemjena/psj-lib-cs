using PsjLib.DDriveFamily;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example03PidTuning
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive PID Tuning Example");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM1");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine($"✓ Connected to device with {device.Channels.Count} channel(s)\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;

        try
        {
            Console.WriteLine("[1] Enabling closed-loop control...");
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
            Console.WriteLine("✓ Closed-loop enabled\n");

            Console.WriteLine("[2] Current PID Parameters:");
            var p = await channel.PidController.GetPAsync().ConfigureAwait(false);
            var i = await channel.PidController.GetIAsync().ConfigureAwait(false);
            var d = await channel.PidController.GetDAsync().ConfigureAwait(false);
            Console.WriteLine($"  P (Proportional): {p:F2}");
            Console.WriteLine($"  I (Integral):     {i:F2}");
            Console.WriteLine($"  D (Derivative):   {d:F2}\n");

            Console.WriteLine("[3] Set new PID settings...");
            await channel.PidController.SetAsync(p: 0.0, i: 0.0, d: 0.0).ConfigureAwait(false);
            Console.WriteLine("  Set: P=0.0, I=0.0, D=0.0\n");

            Console.WriteLine("[4] Moving to test position (50 µm)...");
            await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
            Console.WriteLine("✓ Move command sent\n");

            Console.WriteLine("[5] Restoring original PID settings...");
            await channel.PidController.SetAsync(p: p, i: i, d: d).ConfigureAwait(false);
            Console.WriteLine("✓ Original settings restored\n");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("PID Tuning Tips:");
        Console.WriteLine("- Start with low gains and increase gradually");
        Console.WriteLine("- P gain affects response speed");
        Console.WriteLine("- I gain eliminates steady-state error");
        Console.WriteLine("- D gain reduces overshoot and oscillation");
        Console.WriteLine(new string('=', 60));
    }
}
