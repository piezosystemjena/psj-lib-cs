namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for actuator output voltage.
/// </summary>
public sealed class OutputVoltage(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for output voltage query.
    /// </summary>
    public const string CmdOutputVoltage = "OUTPUT_VOLTAGE";

    /// <summary>
    /// Reads current output voltage.
    /// </summary>
    /// <returns>Output voltage in volts.</returns>
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdOutputVoltage).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
