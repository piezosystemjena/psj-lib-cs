using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public enum DDriveWaveformType
{
    None = 0,
    Sine = 1,
    Triangle = 2,
    Rectangle = 3,
    Noise = 4,
    Sweep = 5,
    Unknown = 99,
}

public enum DDriveScanType
{
    Off = 0,
    SineOnce = 1,
    TriangleOnce = 2,
    SineTwice = 3,
    TriangleTwice = 4,
    Unknown = 99,
}

public sealed class DDriveWaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdWfgType = "WFG_TYPE";
    public const string CmdSineAmplitude = "WFG_SINE_AMPLITUDE";
    public const string CmdSineOffset = "WFG_SINE_OFFSET";
    public const string CmdSineFrequency = "WFG_SINE_FREQUENCY";
    public const string CmdTriAmplitude = "WFG_TRIANGLE_AMPLITUDE";
    public const string CmdTriOffset = "WFG_TRIANGLE_OFFSET";
    public const string CmdTriFrequency = "WFG_TRIANGLE_FREQUENCY";
    public const string CmdTriDutyCycle = "WFG_TRIANGLE_DUTY_CYCLE";
    public const string CmdRecAmplitude = "WFG_RECTANGLE_AMPLITUDE";
    public const string CmdRecOffset = "WFG_RECTANGLE_OFFSET";
    public const string CmdRecFrequency = "WFG_RECTANGLE_FREQUENCY";
    public const string CmdRecDutyCycle = "WFG_RECTANGLE_DUTY_CYCLE";
    public const string CmdNoiseAmplitude = "WFG_NOISE_AMPLITUDE";
    public const string CmdNoiseOffset = "WFG_NOISE_OFFSET";
    public const string CmdSweepAmplitude = "WFG_SWEEP_AMPLITUDE";
    public const string CmdSweepOffset = "WFG_SWEEP_OFFSET";
    public const string CmdSweepTime = "WFG_SWEEP_TIME";
    public const string CmdScanStart = "WFG_SCAN_START";
    public const string CmdScanType = "WFG_SCAN_TYPE";

    private readonly StaticWaveformGenerator _sine = new(writeCb, new Dictionary<string, string>
    {
        [StaticWaveformGenerator.CmdAmplitude] = commands[CmdSineAmplitude],
        [StaticWaveformGenerator.CmdOffset] = commands[CmdSineOffset],
        [StaticWaveformGenerator.CmdFrequency] = commands[CmdSineFrequency],
    });

    private readonly StaticWaveformGenerator _triangle = new(writeCb, new Dictionary<string, string>
    {
        [StaticWaveformGenerator.CmdAmplitude] = commands[CmdTriAmplitude],
        [StaticWaveformGenerator.CmdOffset] = commands[CmdTriOffset],
        [StaticWaveformGenerator.CmdFrequency] = commands[CmdTriFrequency],
        [StaticWaveformGenerator.CmdDutyCycle] = commands[CmdTriDutyCycle],
    });

    private readonly StaticWaveformGenerator _rectangle = new(writeCb, new Dictionary<string, string>
    {
        [StaticWaveformGenerator.CmdAmplitude] = commands[CmdRecAmplitude],
        [StaticWaveformGenerator.CmdOffset] = commands[CmdRecOffset],
        [StaticWaveformGenerator.CmdFrequency] = commands[CmdRecFrequency],
        [StaticWaveformGenerator.CmdDutyCycle] = commands[CmdRecDutyCycle],
    });

    private readonly StaticWaveformGenerator _noise = new(writeCb, new Dictionary<string, string>
    {
        [StaticWaveformGenerator.CmdAmplitude] = commands[CmdNoiseAmplitude],
        [StaticWaveformGenerator.CmdOffset] = commands[CmdNoiseOffset],
    });

    private readonly StaticWaveformGenerator _sweep = new(writeCb, new Dictionary<string, string>
    {
        [StaticWaveformGenerator.CmdAmplitude] = commands[CmdSweepAmplitude],
        [StaticWaveformGenerator.CmdOffset] = commands[CmdSweepOffset],
        [StaticWaveformGenerator.CmdFrequency] = commands[CmdSweepTime],
    });

    public StaticWaveformGenerator Sine => _sine;
    public StaticWaveformGenerator Triangle => _triangle;
    public StaticWaveformGenerator Rectangle => _rectangle;
    public StaticWaveformGenerator Noise => _noise;
    public StaticWaveformGenerator Sweep => _sweep;

    public async Task SetWaveformTypeAsync(DDriveWaveformType waveformType)
        => _ = await WriteAsync(CmdWfgType, [waveformType]).ConfigureAwait(false);

    public async Task<DDriveWaveformType> GetWaveformTypeAsync()
        => (DDriveWaveformType)int.Parse((await WriteAsync(CmdWfgType).ConfigureAwait(false))[0]);

    public async Task StartScanAsync(DDriveScanType scanType)
    {
        _ = await WriteAsync(CmdScanType, [scanType]).ConfigureAwait(false);
        _ = await WriteAsync(CmdScanStart, [1]).ConfigureAwait(false);
    }

    public async Task<bool> IsScanRunningAsync()
        => int.Parse((await WriteAsync(CmdScanStart).ConfigureAwait(false))[0]) != 0;
}
