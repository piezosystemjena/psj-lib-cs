using PsjLib.DDriveFamily;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example06FilterConfiguration
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive Filter Configuration Example");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM1");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Connected to device\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;
        
        try
        {
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

            Console.WriteLine("[1] Configuring Notch Filter...");
            Console.WriteLine("  Purpose: Suppress mechanical resonances");
            await channel.Notch.SetAsync(enabled: true, frequency: 500.0, bandwidth: 50.0).ConfigureAwait(false);
            var notchEnabled = await channel.Notch.GetEnabledAsync().ConfigureAwait(false);
            var notchFreq = await channel.Notch.GetFrequencyAsync().ConfigureAwait(false);
            var notchBw = await channel.Notch.GetBandwidthAsync().ConfigureAwait(false);
            Console.WriteLine($"  ✓ Notch filter: {(notchEnabled ? "Enabled" : "Disabled")}");
            Console.WriteLine($"    Frequency: {notchFreq:F1} Hz, Bandwidth: {notchBw:F1} Hz\n");

            Console.WriteLine("  Testing with notch filter enabled...");
            await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"  Position: {(await channel.Position.GetAsync().ConfigureAwait(false)):F2} µm\n");

            Console.WriteLine("[2] Configuring Low-Pass Filter...");
            Console.WriteLine("  Purpose: Reduce high-frequency noise");
            await channel.Lpf.SetAsync(enabled: true, cutoffFrequency: 100.0).ConfigureAwait(false);
            var lpfEnabled = await channel.Lpf.GetEnabledAsync().ConfigureAwait(false);
            var lpfFreq = await channel.Lpf.GetCutoffFrequencyAsync().ConfigureAwait(false);
            Console.WriteLine($"  ✓ Low-pass filter: {(lpfEnabled ? "Enabled" : "Disabled")}");
            Console.WriteLine($"    Cutoff: {lpfFreq:F1} Hz\n");

            Console.WriteLine("  Testing with low-pass filter enabled...");
            await channel.Setpoint.SetAsync(70.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"  Position: {(await channel.Position.GetAsync().ConfigureAwait(false)):F2} µm\n");

            Console.WriteLine("[3] Configuring Error Low-Pass Filter...");
            Console.WriteLine("  Purpose: Filter position error before PID controller");
            await channel.ErrorLpf.SetAsync(cutoffFrequency: 200.0, order: 2).ConfigureAwait(false);
            var errFreq = await channel.ErrorLpf.GetCutoffFrequencyAsync().ConfigureAwait(false);
            var errOrder = await channel.ErrorLpf.GetOrderAsync().ConfigureAwait(false);
            Console.WriteLine($"  ✓ Error LPF: Cutoff={errFreq:F1} Hz, Order={errOrder}\n");

            Console.WriteLine("  Testing with error filter enabled...");
            await channel.Setpoint.SetAsync(30.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"  Position: {(await channel.Position.GetAsync().ConfigureAwait(false)):F2} µm\n");

            Console.WriteLine("[4] Testing with all filters disabled...");
            await channel.Notch.SetAsync(enabled: false).ConfigureAwait(false);
            await channel.Lpf.SetAsync(enabled: false).ConfigureAwait(false);
            await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"  Position: {(await channel.Position.GetAsync().ConfigureAwait(false)):F2} µm");
            Console.WriteLine("  (Compare response stability with filters off)\n");

            Console.WriteLine("[5] Checking filter status from hardware...");
            var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"  Notch filter active: {status.NotchFilterActive}");
            Console.WriteLine($"  Low-pass filter active: {status.LowPassFilterActive}");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("\n✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Filter Tuning Tips:");
        Console.WriteLine("- Notch: Set to mechanical resonance frequency");
        Console.WriteLine("- LPF: Lower cutoff = smoother but slower response");
        Console.WriteLine("- Error LPF: Helps PID stability, start with 200-500 Hz");
        Console.WriteLine("- Test with small position steps to verify stability");
        Console.WriteLine(new string('=', 60));
    }
}
