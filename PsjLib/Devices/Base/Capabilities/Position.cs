namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for current actuator position.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Position units and interpretation depend on device mode and configuration (for example sensor-based closed-loop feedback versus open-loop drive representation).</para>
/// </remarks>
public sealed class Position(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for querying measured position.
    /// </summary>
    internal const string CmdPosition = "POSITION";

    /// <summary>
    /// Reads the current measured position.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Update cadence is hardware-dependent and values are returned in the device's configured position unit.</para>
    /// </remarks>
    /// <returns>Position value in device-specific units.</returns>
    public async Task<double> GetAsync()
        => double.Parse((await WriteAsync(CmdPosition).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
