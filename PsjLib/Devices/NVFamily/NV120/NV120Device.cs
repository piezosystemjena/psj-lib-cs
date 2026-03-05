using PsjLib.Base;
using PsjLib.NVFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.NVFamily.NV120;

/// <summary>
/// Device implementation for NV120/1 open-loop amplifier.
/// </summary>
public sealed class NV120Device : NVFamilyDevice
{
    static NV120Device()
    {
        DeviceModelRegistry.Registry["NV120/1"] = static (transport, id) => new NV120Device(transport, id);
    }

    /// <summary>
    /// Initializes a new NV120/1 device wrapper.
    /// </summary>
    /// <param name="transportType">Transport backend.</param>
    /// <param name="identifier">Transport identifier.</param>
    public NV120Device(TransportType transportType, string identifier)
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
    }

    /// <inheritdoc/>
    public override string? DeviceId => "NV120/1";
    /// <inheritdoc/>
    internal override string NVFamilyIdentifier => "NV120";
    /// <inheritdoc/>
    public override int MaxChannelCount => 1;
    /// <inheritdoc/>
    internal override NVFamilyChannel CreateChannel(int channelId) => new NV120Channel(channelId, WriteChannelAsync);

    /// <summary>
    /// Gets typed NV120 channels keyed by channel identifier.
    /// </summary>
    public new IReadOnlyDictionary<int, NV120Channel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (NV120Channel)kv.Value);
    /// <summary>
    /// Front-panel knob configuration capability.
    /// </summary>
    public NVKnob Knob { get; }
}
