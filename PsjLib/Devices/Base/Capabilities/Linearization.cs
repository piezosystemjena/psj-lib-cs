namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for enabling or disabling linearization mode.
/// </summary>
public sealed class Linearization(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for linearization enable state.
    /// </summary>
    internal const string CmdEnable = "LINEARIZATION_ENABLE";

    /// <summary>
    /// Enables or disables linearization.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable linearization.</param>
    public async Task SetAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);

    /// <summary>
    /// Reads linearization enable state.
    /// </summary>
    /// <returns><see langword="true"/> when enabled.</returns>
    public async Task<bool> GetEnabledAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
