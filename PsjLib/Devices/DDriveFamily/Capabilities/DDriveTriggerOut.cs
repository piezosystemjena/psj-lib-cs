using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive trigger output capability with additional offset parameter.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Offset is a d-Drive-specific extension used for phase/timing compensation.</para>
/// <para><b>Notes:</b> Trigger timing resolution follows the control loop timing (50kHz nominal).</para>
/// </remarks>
public sealed class DDriveTriggerOut(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : TriggerOut(writeCb, commands)
{
    /// <summary>
    /// Command token for trigger output offset.
    /// </summary>
    internal const string CmdOffset = "TRIGGER_OUT_OFFSET";

    /// <summary>
    /// Updates trigger-out parameters, including optional d-Drive specific offset.
    /// </summary>
    /// <param name="startValue">Optional start threshold.</param>
    /// <param name="stopValue">Optional stop threshold.</param>
    /// <param name="interval">Optional trigger interval.</param>
    /// <param name="length">Optional pulse length.</param>
    /// <param name="edge">Optional trigger polarity.</param>
    /// <param name="src">Optional trigger data source.</param>
    /// <param name="offset">Optional trigger offset.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null arguments are written; unspecified values keep existing device settings.</para>
    /// </remarks>
    public async Task SetAsync(
        double? startValue = null,
        double? stopValue = null,
        double? interval = null,
        int? length = null,
        TriggerEdge? edge = null,
        TriggerDataSource? src = null,
        double? offset = null)
    {
        await base.SetAsync(startValue, stopValue, interval, length, edge, src).ConfigureAwait(false);
        if (offset is not null)
        {
            _ = await WriteAsync(CmdOffset, [offset.Value]).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reads configured trigger output offset.
    /// </summary>
    /// <returns>Trigger offset value.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Offset readback is specific to d-Drive trigger-out extensions.</para>
    /// </remarks>
    public async Task<double> GetOffsetAsync() => double.Parse((await WriteAsync(CmdOffset).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
