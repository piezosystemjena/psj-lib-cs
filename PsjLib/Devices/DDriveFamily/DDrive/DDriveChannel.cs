using PsjLib.Base;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Typed channel class for multi-channel d-Drive devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Inherits all d-Drive family capabilities and timing semantics from <see cref="DDriveFamilyChannel"/>.</para>
/// </remarks>
public sealed class DDriveChannel(int id, ChannelWriteCallback writeCallback) : DDriveFamilyChannel(id, writeCallback)
{
}
