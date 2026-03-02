namespace PsjLib.Base.Capabilities;

/// <summary>
/// Lightweight capability factory descriptor.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> This descriptor provides property-like capability access semantics by wrapping a capability factory and creating instances on demand.</para>
/// </remarks>
public sealed class CapabilityDescriptor<TCapability>
    where TCapability : PiezoCapability
{
    public CapabilityDescriptor(Func<TCapability> factory) => Factory = factory;
    public Func<TCapability> Factory { get; }

    /// <remarks>
    /// <para><b>Notes:</b> This method returns a capability instance from the configured factory for object-level access.</para>
    /// </remarks>
    public TCapability Create() => Factory();
}
