namespace PsjLib.Base.Capabilities;

public delegate void ProgressCallback(int current, int total);
public delegate Task<IReadOnlyList<string>> CapabilityWriteCallback(IReadOnlyDictionary<string, string> deviceCommands, string command, IReadOnlyList<object?>? parameters = null);

public abstract class PiezoCapability
{
    protected readonly CapabilityWriteCallback WriteCallback;
    protected readonly IReadOnlyDictionary<string, string> DeviceCommands;

    protected PiezoCapability(CapabilityWriteCallback writeCallback, IReadOnlyDictionary<string, string> deviceCommands)
    {
        WriteCallback = writeCallback;
        DeviceCommands = deviceCommands;
    }

    protected Task<IReadOnlyList<string>> WriteAsync(string command, IReadOnlyList<object?>? parameters = null)
        => WriteCallback(DeviceCommands, command, parameters);
}
