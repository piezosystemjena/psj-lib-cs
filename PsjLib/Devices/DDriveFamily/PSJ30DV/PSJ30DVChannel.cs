using PsjLib.Base;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Typed single-channel class for PSJ 30DV devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Inherits all d-Drive family channel capabilities while being bound to the single-channel PSJ 30DV topology.</para>
/// </remarks>
public sealed class PSJ30DVChannel(int id, ChannelWriteCallback writeCallback) : DDriveFamilyChannel(id, writeCallback)
{
}
