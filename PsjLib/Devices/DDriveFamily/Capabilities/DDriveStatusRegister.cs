using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// Status bits describing active waveform generator mode.
/// </summary>
public enum DDriveWaveformGeneratorStatus
{
    /// <summary>
    /// Waveform generator inactive.
    /// </summary>
    Inactive = 0,
    /// <summary>
    /// Sine waveform active.
    /// </summary>
    Sine = 1,
    /// <summary>
    /// Triangle waveform active.
    /// </summary>
    Triangle = 2,
    /// <summary>
    /// Rectangle waveform active.
    /// </summary>
    Rectangle = 3,
    /// <summary>
    /// Noise waveform active.
    /// </summary>
    Noise = 4,
    /// <summary>
    /// Sweep waveform active.
    /// </summary>
    Sweep = 5,
    /// <summary>
    /// Unknown waveform mode.
    /// </summary>
    Unknown = 99,
}

/// <summary>
/// Decodes d-Drive status register fields into typed properties.
/// </summary>
/// <param name="raw">Raw status response values.</param>
/// <remarks>
/// <para><b>Notes:</b> Values are decoded from a 16-bit hardware status word and represent read-only state as reported by firmware.</para>
/// </remarks>
public sealed class DDriveStatusRegister(IReadOnlyList<string> raw) : StatusRegister(raw)
{
    private int Value => int.Parse(RawValues[0]);

    /// <summary>
    /// Gets whether an actuator is detected as connected.
    /// </summary>
    public bool ActorPlugged => (Value & 0x0001) != 0;
    /// <summary>
    /// Gets decoded sensor type.
    /// </summary>
    public SensorType SensorType => (SensorType)((Value & 0x0006) >> 1);
    /// <summary>
    /// Gets whether piezo output voltage stage is enabled.
    /// </summary>
    public bool PiezoVoltageEnabled => (Value & 0x0040) != 0;
    /// <summary>
    /// Gets whether closed-loop control is active.
    /// </summary>
    public bool ClosedLoop => (Value & 0x0080) != 0;
    /// <summary>
    /// Gets decoded waveform generator mode from status bits.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Unknown hardware values are mapped to <see cref="DDriveWaveformGeneratorStatus.Unknown"/>.</para>
    /// </remarks>
    public DDriveWaveformGeneratorStatus WaveformGeneratorStatus
    {
        get
        {
            var value = (Value & 0x0E00) >> 9;
            return Enum.IsDefined(typeof(DDriveWaveformGeneratorStatus), value)
                ? (DDriveWaveformGeneratorStatus)value
                : DDriveWaveformGeneratorStatus.Unknown;
        }
    }

    /// <summary>
    /// Gets whether notch filter is active.
    /// </summary>
    public bool NotchFilterActive => (Value & 0x1000) != 0;
    /// <summary>
    /// Gets whether low-pass filter is active.
    /// </summary>
    public bool LowPassFilterActive => (Value & 0x2000) != 0;
}
