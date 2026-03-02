namespace PsjLib.Base.Capabilities;

public abstract class WaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdEnable = "WAVEFORM_GENERATOR_ENABLE";

    public async Task SetEnableAsync(bool enabled) => _ = await WriteAsync(CmdEnable, [enabled]).ConfigureAwait(false);
    public async Task<bool> GetEnableAsync() => int.Parse((await WriteAsync(CmdEnable).ConfigureAwait(false))[0]) != 0;
}
