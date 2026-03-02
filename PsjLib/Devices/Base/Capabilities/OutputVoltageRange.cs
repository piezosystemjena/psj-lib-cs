namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for configured output voltage range.
/// </summary>
public sealed class OutputVoltageRange(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for output voltage range query.
    /// </summary>
    public const string CmdOutputVoltageRange = "OUTPUT_VOLTAGE_RANGE";

    /// <summary>
    /// Reads the output voltage range.
    /// </summary>
    /// <returns>Configured output voltage range value.</returns>
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdOutputVoltageRange).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
