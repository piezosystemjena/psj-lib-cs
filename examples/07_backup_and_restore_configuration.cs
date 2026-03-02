using System.Text.Json;
using PsjLib.DDriveFamily;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example07BackupAndRestoreConfiguration
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("d-Drive Backup and Restore Configuration Example");
        Console.WriteLine(new string('=', 60));

        var device = new DDriveDevice(TransportType.Serial, "COM1");
        await device.ConnectAsync().ConfigureAwait(false);
        Console.WriteLine("✓ Connected to device\n");

        // Get the first available channel
        var channel = device.Channels.First(x => true).Value;

        try
        {
            await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

            Console.WriteLine("[1] Current Configuration:");
            var p = await channel.PidController.GetPAsync().ConfigureAwait(false);
            var i = await channel.PidController.GetIAsync().ConfigureAwait(false);
            var d = await channel.PidController.GetDAsync().ConfigureAwait(false);
            var sr = await channel.SlewRate.GetAsync().ConfigureAwait(false);
            var notchEnabled = await channel.Notch.GetEnabledAsync().ConfigureAwait(false);
            Console.WriteLine($"  PID: P={p:F2}, I={i:F2}, D={d:F2}");
            Console.WriteLine($"  Slew Rate: {sr:F2} V/ms");
            Console.WriteLine($"  Notch Filter: {(notchEnabled ? "Enabled" : "Disabled")}\n");

            Console.WriteLine("[2] Backing up configuration...");
            var backup = await device.BackupAsync().ConfigureAwait(false);
            Console.WriteLine($"  ✓ Backed up {backup.Count} settings");

            const string backupFile = "ddrive_config_backup.json";
            var backupSerializable = backup.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
            await File.WriteAllTextAsync(backupFile, JsonSerializer.Serialize(backupSerializable, new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
            Console.WriteLine($"  ✓ Saved backup to {backupFile}\n");

            Console.WriteLine("[3] Modifying settings for experiment...");
            await channel.PidController.SetAsync(p: 0, i: 0, d: 0).ConfigureAwait(false);
            await channel.SlewRate.SetAsync(5.0).ConfigureAwait(false);
            await channel.Notch.SetAsync(enabled: true, frequency: 500.0, bandwidth: 50.0).ConfigureAwait(false);
            Console.WriteLine("  ✓ Applied experimental settings:");
            Console.WriteLine("    PID: P=0, I=0, D=0");
            Console.WriteLine("    Slew Rate: 5.0 V/ms");
            Console.WriteLine("    Notch Filter: Enabled at 500 Hz\n");

            var pNew = await channel.PidController.GetPAsync().ConfigureAwait(false);
            var iNew = await channel.PidController.GetIAsync().ConfigureAwait(false);
            Console.WriteLine($"  Verified: P={pNew:F2}, I={iNew:F2}\n");

            Console.WriteLine("[4] Running simulated experiment...");
            await channel.Setpoint.SetAsync(40.0).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            await channel.Setpoint.SetAsync(60.0).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            Console.WriteLine("  ✓ Experiment complete\n");

            Console.WriteLine("[5] Restoring original configuration...");
            await device.RestoreAsync(backup).ConfigureAwait(false);
            Console.WriteLine("  ✓ Configuration restored");

            var pRestored = await channel.PidController.GetPAsync().ConfigureAwait(false);
            var iRestored = await channel.PidController.GetIAsync().ConfigureAwait(false);
            var srRestored = await channel.SlewRate.GetAsync().ConfigureAwait(false);
            var notchRestored = await channel.Notch.GetEnabledAsync().ConfigureAwait(false);
            Console.WriteLine($"  Restored: PID P={pRestored:F2}, I={iRestored:F2}");
            Console.WriteLine($"  Restored: Slew Rate={srRestored:F2} V/ms");
            Console.WriteLine($"  Restored: Notch Filter={(notchRestored ? "Enabled" : "Disabled")}\n");

            Console.WriteLine("[6] Loading configuration from file...");
            var loadedJson = await File.ReadAllTextAsync(backupFile).ConfigureAwait(false);
            var loadedConfig = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(loadedJson)
                               ?? new Dictionary<string, List<string>>();
            var loadedForRestore = loadedConfig.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyList<string>)kv.Value);

            await device.RestoreAsync(loadedForRestore).ConfigureAwait(false);
            Console.WriteLine($"  ✓ Loaded and applied configuration from {backupFile}");
        }
        finally
        {
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("\n✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Configuration Management Tips:");
        Console.WriteLine("- Backup before experiments to ensure reproducibility");
        Console.WriteLine("- Save backups to files for different use cases");
        Console.WriteLine("- Use meaningful filenames (e.g., 'scanning_config.json')");
        Console.WriteLine("- Backup includes PID, filters, waveforms, triggers, etc.");
        Console.WriteLine("- Dynamic values (position, setpoint) are NOT backed up");
        Console.WriteLine(new string('=', 60));
    }
}
