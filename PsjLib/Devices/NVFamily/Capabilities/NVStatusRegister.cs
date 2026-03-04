using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV-family status register decoder for per-channel error/status flags.
/// </summary>
public sealed class NVStatusRegister(IReadOnlyList<string> raw, int? channelId = null) : StatusRegister(raw, channelId)
{
    private bool InterpretStatusRegister(int flag)
    {
        if (ChannelId is null)
        {
            throw new InvalidOperationException("Channel ID is required to interpret status register for NV Family.");
        }

        var value = Convert.ToInt32(RawValues[ChannelId.Value], 16);
        return (value & flag) != 0;
    }

    /// <summary>
    /// Gets whether an actuator is detected as connected.
    /// </summary>
    public bool ActuatorPlugged => !InterpretStatusRegister(0x0001);
    /// <summary>
    /// Gets whether an actuator short condition is reported.
    /// </summary>
    public bool ActuatorShort => InterpretStatusRegister(0x0002);
    /// <summary>
    /// Gets whether an EEPROM error is reported.
    /// </summary>
    public bool EepromError => InterpretStatusRegister(0x0010);
    /// <summary>
    /// Gets whether an underload condition is reported.
    /// </summary>
    public bool Underload => InterpretStatusRegister(0x1000);
    /// <summary>
    /// Gets whether an overload condition is reported.
    /// </summary>
    public bool Overload => InterpretStatusRegister(0x2000);
    /// <summary>
    /// Gets whether an invalid actuator condition is reported.
    /// </summary>
    public bool InvalidActuator => InterpretStatusRegister(0x4000);
    /// <summary>
    /// Gets whether an over-temperature condition is reported.
    /// </summary>
    public bool OverTemperature => InterpretStatusRegister(0x8000);
}
