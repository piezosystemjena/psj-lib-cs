namespace PsjLib.Base.Capabilities;

/// <summary>
/// Reports progress for long-running capability reads.
/// </summary>
/// <param name="current">Completed item count.</param>
/// <param name="total">Total item count.</param>
public delegate void ProgressCallback(int current, int total);
/// <summary>
/// Writes a capability command through a channel/device command mapper.
/// </summary>
/// <param name="deviceCommands">Capability-to-device command mapping.</param>
/// <param name="command">Capability command token.</param>
/// <param name="parameters">Optional command parameters.</param>
/// <returns>Parsed response fields.</returns>
public delegate Task<IReadOnlyList<string>> CapabilityWriteCallback(IReadOnlyDictionary<string, string> deviceCommands, string command, IReadOnlyList<object?>? parameters = null);

/// <summary>
/// Base class for channel capabilities.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> This type is intended as a common base for concrete capabilities; command execution is delegated through the configured write callback abstraction.</para>
/// </remarks>
public abstract class PiezoCapability
{
    /// <summary>
    /// Write callback used by the capability to execute mapped device commands.
    /// </summary>
    protected readonly CapabilityWriteCallback WriteCallback;
    /// <summary>
    /// Command mapping for this capability instance.
    /// </summary>
    protected readonly IReadOnlyDictionary<string, string> DeviceCommands;

    /// <summary>
    /// Initializes a capability with command mapping and write callback.
    /// </summary>
    /// <param name="writeCallback">Callback used to execute mapped commands.</param>
    /// <param name="deviceCommands">Mapping from capability tokens to device command strings.</param>
    protected PiezoCapability(CapabilityWriteCallback writeCallback, IReadOnlyDictionary<string, string> deviceCommands)
    {
        WriteCallback = writeCallback;
        DeviceCommands = deviceCommands;
    }

    /// <summary>
    /// Executes a mapped capability command.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Subclasses use this helper to route operations through the callback provided by the owning device/channel context.</para>
    /// </remarks>
    /// <param name="command">Capability command token.</param>
    /// <param name="parameters">Optional command arguments.</param>
    /// <returns>Parsed response fields.</returns>
    protected Task<IReadOnlyList<string>> WriteAsync(string command, IReadOnlyList<object?>? parameters = null)
        => WriteCallback(DeviceCommands, command, parameters);
}
