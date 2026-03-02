using System.Globalization;
using PsjLib.DDriveFamily;
using PsjLib.DDriveFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example04DataRecorderCapture
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive Data Recorder Example");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM6");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Connected to device\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;

        try
        {
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

            Console.WriteLine("[1] Configuring data recorder...");
            const int sampleRate = 10000;
            const double durationSec = 0.1;
            var numSamples = (int)(sampleRate * durationSec);
            var stride = (int)(channel.DataRecorder.SampleRate / sampleRate);

            await channel.DataRecorder.SetAsync(memoryLength: numSamples, stride: stride).ConfigureAwait(false);
            Console.WriteLine($"  ✓ Configured: {numSamples} samples at {sampleRate} Hz");
            Console.WriteLine($"  Duration: {durationSec} seconds\n");

            Console.WriteLine("\n[2] Performing position step (30µm → 70µm)...");
            await channel.Setpoint.SetAsync(30.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            await channel.Setpoint.SetAsync(70.0).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine("  ✓ Motion complete");

            Console.WriteLine("\n[3] Retrieving recorded data...");
            Console.WriteLine("    Retrieving position data...");
            var positionData = await channel.DataRecorder.GetAllDataAsync(
                DDriveDataRecorderChannel.Position,
                numSamples,
                Progress).ConfigureAwait(false);

            Console.WriteLine("\n    Retrieving voltage data...");
            var voltageData = await channel.DataRecorder.GetAllDataAsync(
                DDriveDataRecorderChannel.Voltage,
                numSamples,
                Progress).ConfigureAwait(false);

            Console.WriteLine($"\n  ✓ Retrieved {positionData.Count} position samples");
            Console.WriteLine($"  ✓ Retrieved {voltageData.Count} voltage samples");

            Console.WriteLine("\n[4] Saving data to CSV...");
            const string filename = "recorder_data.csv";
            await using (var writer = new StreamWriter(filename))
            {
                await writer.WriteLineAsync("Time (s),Position (%),Voltage (V)").ConfigureAwait(false);
                for (var i = 0; i < positionData.Count; i++)
                {
                    var t = i / (double)sampleRate;
                    await writer.WriteLineAsync(string.Join(',',
                        t.ToString("F6", CultureInfo.InvariantCulture),
                        positionData[i].ToString("F3", CultureInfo.InvariantCulture),
                        voltageData[i].ToString("F3", CultureInfo.InvariantCulture))).ConfigureAwait(false);
                }
            }
            Console.WriteLine($"  ✓ Saved to {filename}");

            Console.WriteLine("\n[5] Data Statistics:");
            Console.WriteLine($"  Position: min={positionData.Min():F2}, max={positionData.Max():F2}, avg={positionData.Average():F2} %");
            Console.WriteLine($"  Voltage:  min={voltageData.Min():F2}, max={voltageData.Max():F2}, avg={voltageData.Average():F2} V");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("\n✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Data recorder tips:");
        Console.WriteLine("- Maximum 500,000 samples per channel");
        Console.WriteLine("- Both channels record simultaneously");
        Console.WriteLine("- Use stride to reduce sample rate for longer captures");
        Console.WriteLine("- Recording auto-starts on position changes");
        Console.WriteLine(new string('=', 60));

        static void Progress(int current, int total)
        {
            if (total <= 0)
            {
                return;
            }

            var step = Math.Max(1, total / 10);
            if (current % step == 0 || current == total)
            {
                var percent = (current / (double)total) * 100;
                Console.Write($"\r    Retrieving data... {percent:F1}%");
            }
        }
    }
}
