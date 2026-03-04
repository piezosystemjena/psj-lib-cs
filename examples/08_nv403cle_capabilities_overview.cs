using PsjLib.NVFamily.Capabilities;
using PsjLib.NVFamily.NV403CLE;
using PsjLib.Transport;

namespace PsjLib.Examples;

public static class Example08NV403CLECapabilitiesOverview
{
    public static async Task RunAsync()
    {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine("NV403CLE capabilities overview");
        Console.WriteLine(new string('=', 72));

        var device = new NV403CLEDevice(TransportType.Serial, "COM10");
        Dictionary<string, IReadOnlyList<string>>? backupData = null;

        try
        {
            Console.WriteLine("\n[1] Connecting to NV403CLE...");
            await device.ConnectAsync().ConfigureAwait(false);
            Console.WriteLine("✓ Connected");

            Console.WriteLine("\n[2] Backing up current configuration...");
            backupData = await device.BackupAsync().ConfigureAwait(false);
            Console.WriteLine("✓ Backup created");

            var channel = device.Channels[0];

            Console.WriteLine("\n[3] Setting modulation source to SERIAL for all channels...");
            foreach (var currentChannel in device.Channels.Values)
            {
                await currentChannel.ModulationSource.SetAsync(NVModulationSourceTypes.Serial).ConfigureAwait(false);
            }

            var currentSource = (NVModulationSourceTypes)await channel.ModulationSource.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"✓ Modulation source: {currentSource}");

            Console.WriteLine("\n[4] Reading and setting display brightness...");
            var beforeBrightness = await device.Display.GetBrightnessAsync().ConfigureAwait(false);
            Console.WriteLine($"Current brightness: {beforeBrightness:F1}%");
            await device.Display.SetAsync(40.0).ConfigureAwait(false);
            var afterBrightness = await device.Display.GetBrightnessAsync().ConfigureAwait(false);
            Console.WriteLine($"Updated brightness: {afterBrightness:F1}%");

            Console.WriteLine("\n[5] Toggling closed-loop control...");
            var closedLoopBefore = await channel.ClosedLoopController.GetEnabledAsync().ConfigureAwait(false);
            Console.WriteLine($"Closed-loop before: {closedLoopBefore}");
            await channel.ClosedLoopController.SetAsync(!closedLoopBefore).ConfigureAwait(false);
            var closedLoopAfter = await channel.ClosedLoopController.GetEnabledAsync().ConfigureAwait(false);
            Console.WriteLine($"Closed-loop after:  {closedLoopAfter}");

            Console.WriteLine("\n[6] Reading units and limits...");
            var openloopUnit = await channel.OpenloopUnit.GetAsync().ConfigureAwait(false);
            var openloopLimits = await channel.OpenloopLimits.GetRangeAsync().ConfigureAwait(false);
            Console.WriteLine($"Open-loop unit: {openloopUnit}");
            Console.WriteLine($"Open-loop limits: {openloopLimits.Lower} .. {openloopLimits.Upper}");

            var closedloopUnit = await channel.ClosedloopUnit.GetAsync().ConfigureAwait(false);
            var closedloopLimits = await channel.ClosedloopLimits.GetRangeAsync().ConfigureAwait(false);
            Console.WriteLine($"Closed-loop unit: {closedloopUnit}");
            Console.WriteLine($"Closed-loop limits: {closedloopLimits.Lower} .. {closedloopLimits.Upper}");

            Console.WriteLine("\n[7] Reading actuator connected status for all channels...");
            foreach (var (channelId, currentChannel) in device.Channels)
            {
                var status = await currentChannel.Status.GetAsync().ConfigureAwait(false);
                Console.WriteLine($"Channel {channelId}: actuator connected = {status.ActuatorPlugged}");
            }

            Console.WriteLine("\n[8] Using multi_setpoint and multi_position...");
            Console.WriteLine("    Note: multi_setpoint requires all channels to have actuators connected and modulation source set to SERIAL.");
            var setpoints = new[] { 10.0, 20.0, 30.0 };
            await device.MultiSetpoint.SetAsync(setpoints).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(false);
            var positions = await device.MultiPosition.GetAsync().ConfigureAwait(false);
            Console.WriteLine($"Setpoints written: [{string.Join(", ", setpoints)}]");
            Console.WriteLine($"Positions read:    [{string.Join(", ", positions.Select(p => p.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)))}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex}");
        }
        finally
        {
            if (backupData is not null)
            {
                Console.WriteLine("\n[9] Restoring original configuration...");
                try
                {
                    await device.RestoreAsync(backupData).ConfigureAwait(false);
                    Console.WriteLine("✓ Configuration restored");
                }
                catch (Exception restoreException)
                {
                    Console.WriteLine($"⚠ Restore failed: {restoreException.Message}");
                }
            }

            Console.WriteLine("\n[10] Disconnecting...");
            await device.CloseAsync().ConfigureAwait(false);
            Console.WriteLine("✓ Disconnected");
        }

        Console.WriteLine("\n" + new string('=', 72));
        Console.WriteLine("Example completed");
        Console.WriteLine(new string('=', 72));
    }
}
