namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for supported input voltage range.
/// </summary>
public sealed class InputVoltageRange(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for input voltage range query.
    /// </summary>
    public const string CmdInputVoltageRange = "INPUT_VOLTAGE_RANGE";

    /// <summary>
    /// Reads configured input voltage range.
    /// </summary>
    /// <returns>Input voltage range value as reported by firmware.</returns>
    public async Task<double> GetAsync() => double.Parse((await WriteAsync(CmdInputVoltageRange).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
