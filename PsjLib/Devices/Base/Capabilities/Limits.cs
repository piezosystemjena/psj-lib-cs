namespace PsjLib.Base.Capabilities;

/// <summary>
/// Read-only capability for querying lower and upper admissible limits.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Limits are device- and mode-specific (for example open-loop voltage bounds versus closed-loop position bounds).</para>
/// </remarks>
public class Limits(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for upper limit readback.
    /// </summary>
    public const string CmdUpperLimit = "UPPER_LIMIT";
    /// <summary>
    /// Command token for lower limit readback.
    /// </summary>
    public const string CmdLowerLimit = "LOWER_LIMIT";

    /// <summary>
    /// Reads lower admissible limit.
    /// </summary>
    /// <returns>Lower limit value or <see cref="double.NaN"/> if parsing fails.</returns>
    public async Task<double> GetLowerAsync()
    {
        var result = await WriteAsync(CmdLowerLimit).ConfigureAwait(false);
        return double.TryParse(result[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : double.NaN;
    }

    /// <summary>
    /// Reads upper admissible limit.
    /// </summary>
    /// <returns>Upper limit value or <see cref="double.NaN"/> if parsing fails.</returns>
    public async Task<double> GetUpperAsync()
    {
        var result = await WriteAsync(CmdUpperLimit).ConfigureAwait(false);
        return double.TryParse(result[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : double.NaN;
    }

    /// <summary>
    /// Reads lower and upper admissible limits.
    /// </summary>
    /// <returns>Tuple containing lower and upper limit values.</returns>
    public async Task<(double Lower, double Upper)> GetRangeAsync()
    {
        var lower = await GetLowerAsync().ConfigureAwait(false);
        var upper = await GetUpperAsync().ConfigureAwait(false);
        return (lower, upper);
    }
}
