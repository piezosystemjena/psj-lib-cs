using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive modulation source options.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Encoder-based modes apply offsets relative to the commanded setpoint; in combined analog mode an additional 0-10V input contributes to the modulation offset.</para>
/// </remarks>
public enum DDriveModulationSourceTypes
{
    /// <summary>
    /// Serial encoder signal.
    /// </summary>
    SerialEncoder = 0,
    /// <summary>
    /// Serial encoder with analog contribution.
    /// </summary>
    SerialEncoderAnalog = 1,
}
