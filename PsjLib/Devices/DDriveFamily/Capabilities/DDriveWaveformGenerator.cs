using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive waveform type selector.
/// </summary>
public enum DDriveWaveformType
{
    /// <summary>
    /// No waveform selected.
    /// </summary>
    None = 0,
    /// <summary>
    /// Sine waveform.
    /// </summary>
    Sine = 1,
    /// <summary>
    /// Triangle waveform.
    /// </summary>
    Triangle = 2,
    /// <summary>
    /// Rectangle waveform.
    /// </summary>
    Rectangle = 3,
    /// <summary>
    /// Noise waveform.
    /// </summary>
    Noise = 4,
    /// <summary>
    /// Sweep waveform.
    /// </summary>
    Sweep = 5,
    /// <summary>
    /// Unknown waveform type.
    /// </summary>
    Unknown = 99,
}

/// <summary>
/// Scan sequence presets for d-Drive waveform scans.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Scan modes run finite cycle patterns and stop automatically when the selected sequence is complete.</para>
/// </remarks>
public enum DDriveScanType
{
    /// <summary>
    /// Scan disabled.
    /// </summary>
    Off = 0,
    /// <summary>
    /// Run one sine scan.
    /// </summary>
    SineOnce = 1,
    /// <summary>
    /// Run one triangle scan.
    /// </summary>
    TriangleOnce = 2,
    /// <summary>
    /// Run two sine scans.
    /// </summary>
    SineTwice = 3,
    /// <summary>
    /// Run two triangle scans.
    /// </summary>
    TriangleTwice = 4,
    /// <summary>
    /// Unknown scan type.
    /// </summary>
    Unknown = 99,
}

/// <summary>
/// d-Drive waveform generator capability aggregating waveform-specific parameter groups.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Configure waveform parameters before activating a waveform type; only one waveform type can be active at a time.</para>
/// <para><b>Notes:</b> Sweep mappings use the frequency parameter slot for sweep-time semantics.</para>
/// </remarks>
public sealed class DDriveWaveformGenerator(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>Command token for active waveform type.</summary>
    public const string CmdWfgType = "WFG_TYPE";
    /// <summary>Command token for sine amplitude.</summary>
    public const string CmdSineAmplitude = "WFG_SINE_AMPLITUDE";
    /// <summary>Command token for sine offset.</summary>
    public const string CmdSineOffset = "WFG_SINE_OFFSET";
    /// <summary>Command token for sine frequency.</summary>
    public const string CmdSineFrequency = "WFG_SINE_FREQUENCY";
    /// <summary>Command token for triangle amplitude.</summary>
    public const string CmdTriAmplitude = "WFG_TRIANGLE_AMPLITUDE";
    /// <summary>Command token for triangle offset.</summary>
    public const string CmdTriOffset = "WFG_TRIANGLE_OFFSET";
    /// <summary>Command token for triangle frequency.</summary>
    public const string CmdTriFrequency = "WFG_TRIANGLE_FREQUENCY";
    /// <summary>Command token for triangle duty cycle.</summary>
    public const string CmdTriDutyCycle = "WFG_TRIANGLE_DUTY_CYCLE";
    /// <summary>Command token for rectangle amplitude.</summary>
    public const string CmdRecAmplitude = "WFG_RECTANGLE_AMPLITUDE";
    /// <summary>Command token for rectangle offset.</summary>
    public const string CmdRecOffset = "WFG_RECTANGLE_OFFSET";
    /// <summary>Command token for rectangle frequency.</summary>
    public const string CmdRecFrequency = "WFG_RECTANGLE_FREQUENCY";
    /// <summary>Command token for rectangle duty cycle.</summary>
    public const string CmdRecDutyCycle = "WFG_RECTANGLE_DUTY_CYCLE";
    /// <summary>Command token for noise amplitude.</summary>
    public const string CmdNoiseAmplitude = "WFG_NOISE_AMPLITUDE";
    /// <summary>Command token for noise offset.</summary>
    public const string CmdNoiseOffset = "WFG_NOISE_OFFSET";
    /// <summary>Command token for sweep amplitude.</summary>
    public const string CmdSweepAmplitude = "WFG_SWEEP_AMPLITUDE";
    /// <summary>Command token for sweep offset.</summary>
    public const string CmdSweepOffset = "WFG_SWEEP_OFFSET";
    /// <summary>Command token for sweep time/frequency parameter.</summary>
    public const string CmdSweepTime = "WFG_SWEEP_TIME";
    /// <summary>Command token that starts a scan.</summary>
    public const string CmdScanStart = "WFG_SCAN_START";
    /// <summary>Command token for selecting scan type.</summary>
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

    /// <summary>
    /// Gets sine waveform parameter capability.
    /// </summary>
    public StaticWaveformGenerator Sine => _sine;
    /// <summary>
    /// Gets triangle waveform parameter capability.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Triangle duty cycle controls rise/fall asymmetry (50% is symmetric).</para>
    /// </remarks>
    public StaticWaveformGenerator Triangle => _triangle;
    /// <summary>
    /// Gets rectangle waveform parameter capability.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Rectangle duty cycle controls high/low ratio (50% is square).</para>
    /// </remarks>
    public StaticWaveformGenerator Rectangle => _rectangle;
    /// <summary>
    /// Gets noise waveform parameter capability.
    /// </summary>
    public StaticWaveformGenerator Noise => _noise;
    /// <summary>
    /// Gets sweep waveform parameter capability.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> The sweep generator maps sweep-time configuration to the static waveform frequency parameter slot.</para>
    /// </remarks>
    public StaticWaveformGenerator Sweep => _sweep;

    /// <summary>
    /// Sets active waveform type.
    /// </summary>
    /// <param name="waveformType">Waveform type to activate.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Setting <see cref="DDriveWaveformType.None"/> disables waveform output.</para>
    /// </remarks>
    public async Task SetWaveformTypeAsync(DDriveWaveformType waveformType)
        => _ = await WriteAsync(CmdWfgType, [waveformType]).ConfigureAwait(false);

    /// <summary>
    /// Reads active waveform type.
    /// </summary>
    /// <returns>Current waveform type.</returns>
    public async Task<DDriveWaveformType> GetWaveformTypeAsync()
        => (DDriveWaveformType)int.Parse((await WriteAsync(CmdWfgType).ConfigureAwait(false))[0]);

    /// <summary>
    /// Starts a waveform scan using the specified scan preset.
    /// </summary>
    /// <param name="scanType">Scan sequence to execute.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Configure waveform parameters and active waveform type before starting a scan.</para>
    /// </remarks>
    public async Task StartScanAsync(DDriveScanType scanType)
    {
        _ = await WriteAsync(CmdScanType, [scanType]).ConfigureAwait(false);
        _ = await WriteAsync(CmdScanStart, [1]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads whether a waveform scan is currently active.
    /// </summary>
    /// <returns><see langword="true"/> when scan is running.</returns>
    public async Task<bool> IsScanRunningAsync()
        => int.Parse((await WriteAsync(CmdScanStart).ConfigureAwait(false))[0]) != 0;
}
