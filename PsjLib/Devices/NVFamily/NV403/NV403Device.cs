using PsjLib.Base;
using PsjLib.Base.Capabilities;
using PsjLib.NVFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.NVFamily.NV403;

/// <summary>
/// Device implementation for NV40/3 open-loop three-channel amplifier.
/// </summary>
public sealed class NV403Device : NVFamilyDevice
{
    static NV403Device()
    {
        DeviceModelRegistry.Registry["NV40/3"] = static (transport, id) => new NV403Device(transport, id);
    }

    /// <summary>
    /// Initializes a new NV40/3 device wrapper.
    /// </summary>
    /// <param name="transportType">Transport backend.</param>
    /// <param name="identifier">Transport identifier.</param>
    public NV403Device(TransportType transportType, string identifier)
        : base(transportType, identifier)
    {
        Knob = new NVKnob(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [NVKnob.CmdMode] = "encmode",
            [NVKnob.CmdSampleTime] = "enctime",
            [NVKnob.CmdStepLimit] = "enclim",
            [NVKnob.CmdAccelExponent] = "encexp",
            [NVKnob.CmdStepOpenLoop] = "encstol",
        });

        MultiSetpoint = new MultiSetpoint(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [MultiSetpoint.CmdSetpoints] = "setall",
        }, 3);

        MultiPosition = new MultiPosition(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [MultiPosition.CmdPositions] = "measure",
        });
    }

    /// <inheritdoc/>
    public override string? DeviceId => "NV40/3";
    /// <inheritdoc/>
    internal override string NVFamilyIdentifier => "NV403";
    /// <inheritdoc/>
    public override int MaxChannelCount => 3;
    /// <inheritdoc/>
    internal override NVFamilyChannel CreateChannel(int channelId) => new NV403Channel(channelId, WriteChannelAsync);

    /// <summary>
    /// Gets typed NV40/3 channels keyed by channel identifier.
    /// </summary>
    public new IReadOnlyDictionary<int, NV403Channel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (NV403Channel)kv.Value);
    /// <summary>
    /// Front-panel knob configuration capability.
    /// </summary>
    public NVKnob Knob { get; }
    /// <summary>
    /// Synchronous setpoint write capability for all channels.
    /// </summary>
    /// <remarks>
    /// To use this capability, all 3 channels must have an actuator connected and modulation source must be set to "serial". 
    /// Otherwise, the amplifier will ignore the command.
    /// </remarks>
    public MultiSetpoint MultiSetpoint { get; }
    /// <summary>
    /// Synchronous position readback capability for all channels.
    /// </summary>
    public MultiPosition MultiPosition { get; }
}
