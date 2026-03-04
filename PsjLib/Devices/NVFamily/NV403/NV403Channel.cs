namespace PsjLib.NVFamily.NV403;

/// <summary>
/// Open-loop channel implementation for NV40/3 devices.
/// </summary>
public sealed class NV403Channel(int id, PsjLib.Base.ChannelWriteCallback writeCallback) : NVFamilyChannel(id, writeCallback)
{
}
