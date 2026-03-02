namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for the measured input modulation voltage.
/// </summary>
public sealed class InputVoltage(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for input voltage query.
    /// </summary>
    public const string CmdInputVoltage = "INPUT_VOLTAGE";

    /// <summary>
    /// Reads current input voltage.
    /// </summary>
    /// <returns>Input voltage value in volts.</returns>
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdInputVoltage).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
