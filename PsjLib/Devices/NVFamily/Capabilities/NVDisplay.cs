using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV-family display capability with brightness scaling to NV register range.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Public API uses 0..100% while NV protocol expects values in 0..255.</para>
/// </remarks>
public sealed class NVDisplay(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : Display(writeCb, commands)
{
    /// <inheritdoc/>
    public override async Task SetAsync(double? brightness = null)
    {
        if (brightness is null)
        {
            return;
        }

        if (brightness < 0.0 || brightness > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be between 0 and 100");
        }

        var nvBrightness = ConvertBrightness(brightness.Value);
        _ = await WriteAsync(CmdBrightness, [nvBrightness]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<double> GetBrightnessAsync()
    {
        var result = await WriteAsync(CmdBrightness).ConfigureAwait(false);
        var nvBrightness = int.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
        return ParseBrightness(nvBrightness);
    }

    /// <summary>
    /// Converts percentage brightness to NV register scale.
    /// </summary>
    private static int ConvertBrightness(double brightness)
    {
        return (int)((brightness / 100.0) * 255.0);
    }

    /// <summary>
    /// Converts NV register brightness to percentage.
    /// </summary>
    private static double ParseBrightness(int nvBrightness)
    {
        return (nvBrightness / 255.0) * 100.0;
    }
}
