namespace PsjLib.Base.Capabilities;

public sealed class CapabilityDescriptor<TCapability>
    where TCapability : PiezoCapability
{
    public CapabilityDescriptor(Func<TCapability> factory) => Factory = factory;
    public Func<TCapability> Factory { get; }
    public TCapability Create() => Factory();
}
