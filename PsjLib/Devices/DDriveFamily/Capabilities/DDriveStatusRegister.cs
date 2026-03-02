using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public enum DDriveWaveformGeneratorStatus
{
    Inactive = 0,
    Sine = 1,
    Triangle = 2,
    Rectangle = 3,
    Noise = 4,
    Sweep = 5,
    Unknown = 99,
}

public sealed class DDriveStatusRegister(IReadOnlyList<string> raw) : StatusRegister(raw)
{
    private int Value => int.Parse(RawValues[0]);

    public bool ActorPlugged => (Value & 0x0001) != 0;
    public SensorType SensorType => (SensorType)((Value & 0x0006) >> 1);
    public bool PiezoVoltageEnabled => (Value & 0x0040) != 0;
    public bool ClosedLoop => (Value & 0x0080) != 0;
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

    public bool NotchFilterActive => (Value & 0x1000) != 0;
    public bool LowPassFilterActive => (Value & 0x2000) != 0;
}
