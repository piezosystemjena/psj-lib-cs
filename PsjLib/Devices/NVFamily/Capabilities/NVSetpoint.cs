using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV-family setpoint capability with client-side readback cache.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> NV hardware does not provide direct setpoint readback for this command family, so <see cref="GetAsync"/> returns the most recently written value.</para>
/// </remarks>
public sealed class NVSetpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : Setpoint(writeCb, commands)
{
    private double _setpointCache;

    /// <inheritdoc/>
    public override async Task SetAsync(double setpoint)
    {
        await base.SetAsync(setpoint).ConfigureAwait(false);
        _setpointCache = setpoint;
    }

    /// <summary>
    /// Gets the last setpoint value written through this capability.
    /// </summary>
    /// <returns>Last configured setpoint value.</returns>
    public Task<double> GetAsync()
    {
        return Task.FromResult(_setpointCache);
    }
}
