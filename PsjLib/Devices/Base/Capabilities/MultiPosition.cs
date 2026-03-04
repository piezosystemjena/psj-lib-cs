namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for synchronous multi-channel position readback.
/// </summary>
public class MultiPosition(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for multi-position readback.
    /// </summary>
    public const string CmdPositions = "POSITIONS";

    /// <summary>
    /// Reads current positions for all channels in a single device command.
    /// </summary>
    /// <returns>List of channel position values in device-reported channel order.</returns>
    public async Task<IReadOnlyList<double>> GetAsync()
    {
        var result = await WriteAsync(CmdPositions).ConfigureAwait(false);
        return result.Select(v => double.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToList();
    }
}
