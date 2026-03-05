using PsjLib.Base;
using PsjLib.Base.Capabilities;
using PsjLib.NVFamily.Capabilities;

namespace PsjLib.NVFamily;

/// <summary>
/// Base channel implementation for NV-series devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Some commands are protocol-global on NV hardware even though they conceptually belong to a channel; these are handled without channel prefix.</para>
/// </remarks>
public class NVFamilyChannel : PiezoChannel
{
    /// <inheritdoc/>
    internal override ISet<string> BackupCommands { get; } = new HashSet<string>
    {
        "monwpa", "setk", "cloop",
    };

    internal static readonly ISet<string> GlobalCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ERROR", "dspvmin", "dspvmax",
    };

    /// <summary>
    /// Initializes a new NV-family channel and its shared capabilities.
    /// </summary>
    /// <param name="id">Channel identifier.</param>
    /// <param name="writeCallback">Channel write callback.</param>
    public NVFamilyChannel(int id, ChannelWriteCallback writeCallback)
        : base(id, writeCallback)
    {
        Setpoint = new NVSetpoint(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.Setpoint.CmdSetpoint] = "set",
        });

        Position = new Position(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Position.CmdPosition] = "rk",
        });

        ModulationSource = new NVModulationSource(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.ModulationSource.CmdSource] = "setk",
        });

        MonitorOutput = new NVMonitorOutput(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [PsjLib.Base.Capabilities.MonitorOutput.CmdOutputSrc] = "monwpa",
        });

        OpenloopUnit = new Unit(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Unit.CmdUnit] = "unitol",
        });

        OpenloopLimits = new Limits(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Limits.CmdLowerLimit] = "dspvmin",
            [Limits.CmdUpperLimit] = "dspvmax",
        });

        Status = new Status<NVStatusRegister>(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Status<NVStatusRegister>.CmdStatus] = "ERROR",
        }, id);
    }

    /// <summary>Setpoint capability with cached readback semantics.</summary>
    /// <remarks>The readback value is the last successfully set value, due to the lack of a read command for this setting in the device protocol.</remarks>
    public NVSetpoint Setpoint { get; }
    /// <summary>Measured position capability.</summary>
    public Position Position { get; }
    /// <summary>Modulation source capability with cached readback semantics.</summary>
    /// <remarks>The readback value is the last successfully set value, due to the lack of a read command for this setting in the device protocol.</remarks>
    public NVModulationSource ModulationSource { get; }
    /// <summary>Monitor output routing capability with cached readback semantics.</summary>
    /// <remarks>The readback value is the last successfully set value, due to the lack of a read command for this setting in the device protocol.</remarks>
    public NVMonitorOutput MonitorOutput { get; }
    /// <summary>Open-loop unit query capability.</summary>
    public Unit OpenloopUnit { get; }
    /// <summary>Open-loop lower/upper limit capability.</summary>
    public Limits OpenloopLimits { get; }
    /// <summary>NV status register capability.</summary>
    public Status<NVStatusRegister> Status { get; }
}
