namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring and reading device display brightness.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Brightness values are expressed in percent in the range 0..100 unless a device-specific capability overrides mapping behavior.</para>
/// </remarks>
public class Display(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for display brightness read/write.
    /// </summary>
    internal const string CmdBrightness = "DISPLAY_BRIGHTNESS";

    /// <summary>
    /// Sets display brightness.
    /// </summary>
    /// <param name="brightness">Brightness in percent (0..100). If <see langword="null"/>, no command is sent.</param>
    /// <exception cref="ArgumentOutOfRangeException">Raised when <paramref name="brightness"/> is outside 0..100.</exception>
    public virtual async Task SetAsync(double? brightness = null)
    {
        if (brightness is null)
        {
            return;
        }

        if (brightness < 0.0 || brightness > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be between 0 and 100");
        }

        _ = await WriteAsync(CmdBrightness, [brightness]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads current display brightness.
    /// </summary>
    /// <returns>Brightness in percent (0..100).</returns>
    public virtual async Task<double> GetBrightnessAsync()
    {
        var result = await WriteAsync(CmdBrightness).ConfigureAwait(false);
        return double.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
    }
}
