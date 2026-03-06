namespace PsjLib.Base.Capabilities;

/// <summary>
/// Base capability for enabling/disabling waveform generation output.
/// </summary>
public abstract class WaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for waveform generator enable state.
    /// </summary>
    internal const string CmdEnable = "WAVEFORM_GENERATOR_ENABLE";

    /// <summary>
    /// Enables or disables waveform generation.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable waveform output.</param>
    public async Task SetEnableAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);

    /// <summary>
    /// Reads waveform generation enable state.
    /// </summary>
    /// <returns><see langword="true"/> when waveform generation is enabled.</returns>
    public async Task<bool> GetEnableAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
