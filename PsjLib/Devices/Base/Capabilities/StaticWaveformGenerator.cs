namespace PsjLib.Base.Capabilities;

/// <summary>
/// Configures parameters for a specific static waveform type.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Supported parameters depend on the concrete waveform mapping; for example some waveforms ignore duty cycle and sweep-style mappings may use the frequency slot for time.</para>
/// </remarks>
public sealed class StaticWaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for waveform amplitude.
    /// </summary>
    internal const string CmdAmplitude = "WFG_AMPLITUDE";
    /// <summary>
    /// Command token for waveform offset.
    /// </summary>
    internal const string CmdOffset = "WFG_OFFSET";
    /// <summary>
    /// Command token for waveform frequency or period parameter.
    /// </summary>
    internal const string CmdFrequency = "WFG_FREQUENCY";
    /// <summary>
    /// Command token for waveform duty cycle.
    /// </summary>
    internal const string CmdDutyCycle = "WFG_DUTY_CYCLE";

    /// <summary>
    /// Updates one or more waveform parameters.
    /// </summary>
    /// <param name="amplitude">Optional waveform amplitude.</param>
    /// <param name="offset">Optional waveform offset.</param>
    /// <param name="frequency">Optional waveform frequency parameter.</param>
    /// <param name="dutyCycle">Optional duty cycle (for waveform types that support it).</param>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null parameters are sent to the device.</para>
    /// </remarks>
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
