namespace PsjLib.Base.Capabilities;

public sealed class ActuatorDescription(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdDescription = "actuator_description";
    public async Task<string> GetAsync() => (await WriteAsync(CmdDescription).ConfigureAwait(false))[0];
}
