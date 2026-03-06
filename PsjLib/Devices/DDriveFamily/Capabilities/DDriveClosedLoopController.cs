using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive closed-loop capability with status-register based readback.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> d-Drive closed-loop status is read from hardware status-register bit 7 at a 20µs sample period (50kHz nominal control loop).</para>
/// </remarks>
public sealed class DDriveClosedLoopController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int samplePeriod)
    : ClosedLoopController(writeCb, commands, samplePeriod)
{
    /// <summary>
    /// Command token used to read status register for closed-loop state.
    /// </summary>
    internal const string CmdStatus = "STATUS";

    /// <summary>
    /// Reads closed-loop enable state from status register bit 7.
    /// </summary>
    /// <returns><see langword="true"/> when closed-loop control is active.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> This method performs a hardware status read and does not use a cached enable flag.</para>
    /// </remarks>
    public override async Task<bool> GetEnabledAsync()
        => (int.Parse((await WriteAsync(CmdStatus).ConfigureAwait(false))[0]) & 0x80) != 0;
}
