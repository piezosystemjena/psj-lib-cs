namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for controlling and querying cooling fan state.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Fan control availability is device-dependent, and disabling fan cooling can reduce thermal headroom under sustained load.</para>
/// </remarks>
public sealed class Fan(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for fan enable/disable.
    /// </summary>
    internal const string CmdEnable = "FAN_ENABLE";

    /// <summary>
    /// Enables or disables the fan.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Disable fan mode only with thermal monitoring in place on hardware that permits software fan control.</para>
    /// </remarks>
    /// <param name="enabled"><see langword="true"/> to enable fan operation.</param>
    public async Task SetAsync(bool enabled)
        => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);

    /// <summary>
    /// Reads current fan enable state.
    /// </summary>
    /// <returns><see langword="true"/> when fan is enabled.</returns>
    public async Task<bool> GetEnabledAsync()
        => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
