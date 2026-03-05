using PsjLib.Base;
using PsjLib.Base.Capabilities;
using PsjLib.DDriveFamily.Capabilities;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Channel implementation for d-Drive family devices with pre-wired capability objects.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> d-Drive family channels operate with a 20µs control period (50kHz) used by control, waveform, trigger, and recorder features.</para>
/// <para><b>Notes:</b> Capability instances are model-specific and exposed as strongly typed properties on this class.</para>
/// </remarks>
public class DDriveFamilyChannel : PiezoChannel
{
    /// <summary>
    /// Control sample period in microseconds for this family.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> 20µs corresponds to a nominal 50kHz loop/update rate.</para>
    /// </remarks>
    public const int SamplePeriod = 20;

    /// <inheritdoc/>
    internal override ISet<string> BackupCommands { get; } = new HashSet<string>
    {
        "modon", "monsrc", "cl", "sr", "pcf", "errlpf", "elpor", "kp", "ki", "kd", "tf",
        "notchon", "notchf", "notchb", "lpon", "lpf", "gfkt", "gasin", "gosin", "gfsin", "gatri",
        "gotri", "gftri", "gstri", "garec", "gorec", "gfrec", "gsrec", "ganoi", "gonoi", "gaswe",
        "goswe", "gtswe", "sct", "trgss", "trgse", "trgsi", "trglen", "trgedge", "trgsrc", "trgos",
    };

    /// <summary>
    /// Initializes a d-Drive family channel and all supported capabilities.
    /// </summary>
    /// <param name="id">Channel identifier.</param>
    /// <param name="writeCallback">Callback used to execute channel commands.</param>
    /// <remarks>
    /// <para><b>Notes:</b> Backup command coverage includes persistent configuration commands; dynamic run-time state is not treated as backup configuration.</para>
    /// </remarks>
    public DDriveFamilyChannel(int id, ChannelWriteCallback writeCallback)
        : base(id, writeCallback)
    {
        StatusRegister = new Status<DDriveStatusRegister>(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Status<DDriveStatusRegister>.CmdStatus] = "stat",
        });

        ActuatorDescription = new ActuatorDescription(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [ActuatorDescription.CmdDescription] = "acdescr",
        });

        Setpoint = new DDriveSetpoint(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.Setpoint.CmdSetpoint] = "set",
        });

        Position = new Position(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Position.CmdPosition] = "mess",
        });

        Temperature = new Temperature(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Temperature.CmdTemperature] = "ktemp",
        });

        Fan = new Fan(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Fan.CmdEnable] = "fan",
        });

        ModulationSource = new ModulationSource(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [ModulationSource.CmdSource] = "modon",
        }, typeof(DDriveModulationSourceTypes));

        MonitorOutput = new MonitorOutput(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [MonitorOutput.CmdOutputSrc] = "monsrc",
        }, typeof(DDriveMonitorOutputSource));

        ClosedLoopController = new DDriveClosedLoopController(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.ClosedLoopController.CmdEnable] = "cl",
            [DDriveClosedLoopController.CmdStatus] = "stat",
        }, SamplePeriod);

        SlewRate = new SlewRate(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [SlewRate.CmdRate] = "sr",
        });

        Pcf = new PreControlFactor(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PreControlFactor.CmdValue] = "pcf",
        });

        ErrorLpf = new ErrorLowPassFilter(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [ErrorLowPassFilter.CmdCutoffFrequency] = "errlpf",
            [ErrorLowPassFilter.CmdOrder] = "elpor",
        });

        PidController = new PIDController(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PIDController.CmdP] = "kp",
            [PIDController.CmdI] = "ki",
            [PIDController.CmdD] = "kd",
            [PIDController.CmdTf] = "tf",
        });

        Notch = new NotchFilter(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [NotchFilter.CmdEnable] = "notchon",
            [NotchFilter.CmdFrequency] = "notchf",
            [NotchFilter.CmdBandwidth] = "notchb",
        });

        Lpf = new LowPassFilter(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [LowPassFilter.CmdEnable] = "lpon",
            [LowPassFilter.CmdCutoffFrequency] = "lpf",
        });

        TriggerOut = new DDriveTriggerOut(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.TriggerOut.CmdStart] = "trgss",
            [PsjLib.Base.Capabilities.TriggerOut.CmdStop] = "trgse",
            [PsjLib.Base.Capabilities.TriggerOut.CmdInterval] = "trgsi",
            [PsjLib.Base.Capabilities.TriggerOut.CmdLength] = "trglen",
            [PsjLib.Base.Capabilities.TriggerOut.CmdEdge] = "trgedge",
            [PsjLib.Base.Capabilities.TriggerOut.CmdSrc] = "trgsrc",
            [DDriveTriggerOut.CmdOffset] = "trgos",
        });

        DataRecorder = new DDriveDataRecorder(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.DataRecorder.CmdStartRecording] = "recstart",
            [PsjLib.Base.Capabilities.DataRecorder.CmdStride] = "recstride",
            [PsjLib.Base.Capabilities.DataRecorder.CmdMemoryLength] = "reclen",
            [PsjLib.Base.Capabilities.DataRecorder.CmdPtr] = "recrdptr",
            [PsjLib.Base.Capabilities.DataRecorder.CmdGetData1] = "m",
            [PsjLib.Base.Capabilities.DataRecorder.CmdGetData2] = "u",
        }, SamplePeriod);

        WaveformGenerator = new DDriveWaveformGenerator(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [DDriveWaveformGenerator.CmdWfgType] = "gfkt",
            [DDriveWaveformGenerator.CmdSineAmplitude] = "gasin",
            [DDriveWaveformGenerator.CmdSineOffset] = "gosin",
            [DDriveWaveformGenerator.CmdSineFrequency] = "gfsin",
            [DDriveWaveformGenerator.CmdTriAmplitude] = "gatri",
            [DDriveWaveformGenerator.CmdTriOffset] = "gotri",
            [DDriveWaveformGenerator.CmdTriFrequency] = "gftri",
            [DDriveWaveformGenerator.CmdTriDutyCycle] = "gstri",
            [DDriveWaveformGenerator.CmdRecAmplitude] = "garec",
            [DDriveWaveformGenerator.CmdRecOffset] = "gorec",
            [DDriveWaveformGenerator.CmdRecFrequency] = "gfrec",
            [DDriveWaveformGenerator.CmdRecDutyCycle] = "gsrec",
            [DDriveWaveformGenerator.CmdNoiseAmplitude] = "ganoi",
            [DDriveWaveformGenerator.CmdNoiseOffset] = "gonoi",
            [DDriveWaveformGenerator.CmdSweepAmplitude] = "gaswe",
            [DDriveWaveformGenerator.CmdSweepOffset] = "goswe",
            [DDriveWaveformGenerator.CmdSweepTime] = "gtswe",
            [DDriveWaveformGenerator.CmdScanStart] = "ss",
            [DDriveWaveformGenerator.CmdScanType] = "sct",
        });
    }

    /// <summary>Typed status capability.</summary>
    public Status<DDriveStatusRegister> StatusRegister { get; }
    /// <summary>Actuator description capability.</summary>
    public ActuatorDescription ActuatorDescription { get; }
    /// <summary>Setpoint capability with d-Drive cache semantics.</summary>
    public DDriveSetpoint Setpoint { get; }
    /// <summary>Measured position capability.</summary>
    public Position Position { get; }
    /// <summary>Temperature capability.</summary>
    public Temperature Temperature { get; }
    /// <summary>Fan control capability.</summary>
    public Fan Fan { get; }
    /// <summary>Modulation source capability.</summary>
    public ModulationSource ModulationSource { get; }
    /// <summary>Monitor output source capability.</summary>
    public MonitorOutput MonitorOutput { get; }
    /// <summary>Closed-loop controller capability.</summary>
    public DDriveClosedLoopController ClosedLoopController { get; }
    /// <summary>Slew-rate capability.</summary>
    public SlewRate SlewRate { get; }
    /// <summary>Pre-control factor capability.</summary>
    public PreControlFactor Pcf { get; }
    /// <summary>Error low-pass filter capability.</summary>
    public ErrorLowPassFilter ErrorLpf { get; }
    /// <summary>PID controller capability.</summary>
    public PIDController PidController { get; }
    /// <summary>Notch filter capability.</summary>
    public NotchFilter Notch { get; }
    /// <summary>Low-pass filter capability.</summary>
    public LowPassFilter Lpf { get; }
    /// <summary>Trigger output capability.</summary>
    public DDriveTriggerOut TriggerOut { get; }
    /// <summary>Data recorder capability.</summary>
    public DDriveDataRecorder DataRecorder { get; }
    /// <summary>Waveform generator capability.</summary>
    public DDriveWaveformGenerator WaveformGenerator { get; }

    /// <inheritdoc/>
    public override async Task<Dictionary<string, IReadOnlyList<string>>> BackupAsync()
    {
        var data = await base.BackupAsync().ConfigureAwait(false);
        data["cl"] = [Convert.ToInt32(await ClosedLoopController.GetEnabledAsync().ConfigureAwait(false)).ToString()];
        return data;
    }
}
