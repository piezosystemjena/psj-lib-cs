using PsjLib.Base;

namespace PsjLib.DDriveFamily;

public sealed class DDriveChannel(int id, ChannelWriteCallback writeCallback) : DDriveFamilyChannel(id, writeCallback)
{
}
