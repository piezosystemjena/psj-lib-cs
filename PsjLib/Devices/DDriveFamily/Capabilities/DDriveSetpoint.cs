using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public sealed class DDriveSetpoint(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : Setpoint(writeCb, commands)
{
    private double _setpointCache;

    public override async Task SetAsync(double setpoint)
    {
        _setpointCache = setpoint;
        await base.SetAsync(setpoint).ConfigureAwait(false);
    }

    public Task<double> GetAsync() => Task.FromResult(_setpointCache);
}
