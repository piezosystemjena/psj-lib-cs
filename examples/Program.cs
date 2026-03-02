using PsjLib.Examples;

var examples = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    ["01"] = Example01DeviceDiscoveryAndConnection.RunAsync,
    ["02"] = Example02SimplePositionControl.RunAsync,
    ["03"] = Example03PidTuning.RunAsync,
    ["04"] = Example04DataRecorderCapture.RunAsync,
    ["05"] = Example05WaveformGenerationBasics.RunAsync,
    ["06"] = Example06FilterConfiguration.RunAsync,
    ["07"] = Example07BackupAndRestoreConfiguration.RunAsync,
};

if (args.Length == 0 || !examples.TryGetValue(args[0], out var selected))
{
    Console.WriteLine("Usage: dotnet run --project cs-src/examples -- <example-number>");
    Console.WriteLine("Available examples: 01, 02, 03, 04, 05, 06, 07");
    return;
}

await selected();
