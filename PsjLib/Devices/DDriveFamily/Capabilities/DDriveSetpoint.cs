using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive setpoint capability that maintains a local cached value.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> d-Drive firmware does not provide direct setpoint readback; <see cref="GetAsync"/> therefore returns the last value written through <see cref="SetAsync(double)"/>.</para>
/// <para><b>Notes:</b> Cache values can become stale when setpoints are changed by another application or controller path.</para>
/// </remarks>
public sealed class DDriveSetpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : Setpoint(writeCb, commands)
{
    private double _setpointCache;

    /// <summary>
    /// Writes a new setpoint and updates local cache.
    /// </summary>
    /// <param name="setpoint">Target setpoint value.</param>
    public override async Task SetAsync(double setpoint)
    {
        _setpointCache = setpoint;
        await base.SetAsync(setpoint).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the locally cached setpoint value.
    /// </summary>
    /// <returns>Last setpoint written via <see cref="SetAsync(double)"/>.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> This does not perform a hardware read on d-Drive devices.</para>
    /// </remarks>
    public Task<double> GetAsync() => Task.FromResult(_setpointCache);
}
