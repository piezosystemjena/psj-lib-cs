namespace PsjLib.Base.Capabilities;

public sealed class StaticWaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdAmplitude = "WFG_AMPLITUDE";
    public const string CmdOffset = "WFG_OFFSET";
    public const string CmdFrequency = "WFG_FREQUENCY";
    public const string CmdDutyCycle = "WFG_DUTY_CYCLE";

    public async Task SetAsync(double? amplitude = null, double? offset = null, double? frequency = null, double? dutyCycle = null)
    {
        if (amplitude is not null)
            _ = await WriteAsync(CmdAmplitude, [amplitude.Value]).ConfigureAwait(false);
        if (offset is not null)
            _ = await WriteAsync(CmdOffset, [offset.Value]).ConfigureAwait(false);
        if (frequency is not null)
            _ = await WriteAsync(CmdFrequency, [frequency.Value]).ConfigureAwait(false);
        if (dutyCycle is not null)
            _ = await WriteAsync(CmdDutyCycle, [dutyCycle.Value]).ConfigureAwait(false);
    }
}
