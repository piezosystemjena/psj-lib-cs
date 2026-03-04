namespace PsjLib.NVFamily.NV120;

/// <summary>
/// Single-channel open-loop NV120 channel.
/// </summary>
public sealed class NV120Channel(int id, PsjLib.Base.ChannelWriteCallback writeCallback) : NVFamilyChannel(id, writeCallback)
{
}
