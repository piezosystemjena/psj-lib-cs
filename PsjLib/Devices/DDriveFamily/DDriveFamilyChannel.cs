using PsjLib.Base;
using PsjLib.Base.Capabilities;
using PsjLib.DDriveFamily.Capabilities;

namespace PsjLib.DDriveFamily;

public class DDriveFamilyChannel : PiezoChannel
{
    public const int SamplePeriod = 20;

    public override ISet<string> BackupCommands { get; } = new HashSet<string>
    {
        "modon", "monsrc", "cl", "sr", "pcf", "errlpf", "elpor", "kp", "ki", "kd", "tf",
        "notchon", "notchf", "notchb", "lpon", "lpf", "gfkt", "gasin", "gosin", "gfsin", "gatri",
        "gotri", "gftri", "gstri", "garec", "gorec", "gfrec", "gsrec", "ganoi", "gonoi", "gaswe",
        "goswe", "gtswe", "sct", "trgss", "trgse", "trgsi", "trglen", "trgedge", "trgsrc", "trgos",
    };

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

    public Status<DDriveStatusRegister> StatusRegister { get; }
    public ActuatorDescription ActuatorDescription { get; }
    public DDriveSetpoint Setpoint { get; }
    public Position Position { get; }
    public Temperature Temperature { get; }
    public Fan Fan { get; }
    public ModulationSource ModulationSource { get; }
    public MonitorOutput MonitorOutput { get; }
    public DDriveClosedLoopController ClosedLoopController { get; }
    public SlewRate SlewRate { get; }
    public PreControlFactor Pcf { get; }
    public ErrorLowPassFilter ErrorLpf { get; }
    public PIDController PidController { get; }
    public NotchFilter Notch { get; }
    public LowPassFilter Lpf { get; }
    public DDriveTriggerOut TriggerOut { get; }
    public DDriveDataRecorder DataRecorder { get; }
    public DDriveWaveformGenerator WaveformGenerator { get; }

    public override async Task<Dictionary<string, IReadOnlyList<string>>> BackupAsync()
    {
        var data = await base.BackupAsync().ConfigureAwait(false);
        data["cl"] = [Convert.ToInt32(await ClosedLoopController.GetEnabledAsync().ConfigureAwait(false)).ToString()];
        return data;
    }
}
