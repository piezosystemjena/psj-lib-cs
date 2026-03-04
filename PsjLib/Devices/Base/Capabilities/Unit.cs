namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for querying measurement units of a specific device mode/domain.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Returned units can vary by model and operation mode (for example V, mV, µm, or mrad).</para>
/// </remarks>
public class Unit(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for unit readback.
    /// </summary>
    public const string CmdUnit = "UNIT";

    /// <summary>
    /// Reads active unit string.
    /// </summary>
    /// <returns>Unit string or <c>Unknown</c> when no value is returned.</returns>
    public async Task<string> GetAsync()
    {
        var result = await WriteAsync(CmdUnit).ConfigureAwait(false);
        if (result.Count == 0)
        {
            return "Unknown";
        }

        return result[0].Trim();
    }
}
