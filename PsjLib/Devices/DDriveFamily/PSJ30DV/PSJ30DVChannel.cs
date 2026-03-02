using PsjLib.Base;

namespace PsjLib.DDriveFamily;

public sealed class PSJ30DVChannel(int id, ChannelWriteCallback writeCallback) : DDriveFamilyChannel(id, writeCallback)
{
}
