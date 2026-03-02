namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for controller or sensor temperature.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Temperature source location and unit conventions are device-dependent.</para>
/// </remarks>
public sealed class Temperature(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for querying temperature.
    /// </summary>
    public const string CmdTemperature = "TEMPERATURE";

    /// <summary>
    /// Reads temperature from the device.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Sampling cadence is firmware/device dependent and suited to monitoring rather than high-rate control feedback.</para>
    /// </remarks>
    /// <returns>Temperature value as reported by firmware.</returns>
    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdTemperature).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
