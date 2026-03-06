using PsjLib.DDriveFamily;
using PsjLib.DDriveFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example05WaveformGenerationBasics
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive Waveform Generation Example");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM1");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Connected to device\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;
        var wfg = channel.WaveformGenerator;

        try
        {
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

            Console.WriteLine("[1] Generating Sine Wave...");
            await wfg.Sine.SetAsync(amplitude: 20.0, offset: 20.0, frequency: 5.0).ConfigureAwait(false);
            await wfg.SetWaveformTypeAsync(DDriveWaveformType.Sine).ConfigureAwait(false);
            Console.WriteLine("  ✓ Sine: 20µm amplitude, 20µm offset, 5Hz");
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            Console.WriteLine("[2] Generating Triangle Wave...");
            await wfg.Triangle.SetAsync(amplitude: 30.0, offset: 20.0, frequency: 2.0, dutyCycle: 50.0).ConfigureAwait(false);
            await wfg.SetWaveformTypeAsync(DDriveWaveformType.Triangle).ConfigureAwait(false);
            Console.WriteLine("  ✓ Triangle: 30µm amplitude, 20µm offset, 2Hz, 50% duty");
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            Console.WriteLine("[3] Generating Sweep...");
            await wfg.Sweep.SetAsync(amplitude: 40.0, offset: 30.0, frequency: 3.0).ConfigureAwait(false);
            await wfg.SetWaveformTypeAsync(DDriveWaveformType.Sweep).ConfigureAwait(false);
            Console.WriteLine("  ✓ Sweep: 40µm range, 30µm start, 3s duration");
            await Task.Delay(TimeSpan.FromSeconds(3.5)).ConfigureAwait(false);

            Console.WriteLine("\n[4] Stopping waveform generation...");
            await wfg.SetWaveformTypeAsync(DDriveWaveformType.None).ConfigureAwait(false);
            Console.WriteLine("  ✓ Waveform stopped");

            await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
            Console.WriteLine("  ✓ Returned to 50µm");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("\n✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("D-Drive Waveform Types Available:");
        Console.WriteLine("- SINE: Smooth periodic motion");
        Console.WriteLine("- TRIANGLE: Linear ramps with adjustable duty cycle");
        Console.WriteLine("- RECTANGLE: Square wave positioning");
        Console.WriteLine("- NOISE: Random dithering");
        Console.WriteLine("- SWEEP: Single linear ramp");
        Console.WriteLine(new string('=', 60));
    }
}
