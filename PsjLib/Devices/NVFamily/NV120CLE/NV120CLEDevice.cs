using PsjLib.Base;
using PsjLib.NVFamily.Capabilities;
using PsjLib.Transport;

namespace PsjLib.NVFamily.NV120CLE;

/// <summary>
/// Device implementation for NV120CLE closed-loop amplifier.
/// </summary>
public sealed class NV120CLEDevice : NVFamilyDevice
{
    static NV120CLEDevice()
    {
        DeviceModelRegistry.Registry["NV120CLE"] = static (transport, id) => new NV120CLEDevice(transport, id);
    }

    /// <summary>
    /// Initializes a new NV120CLE device wrapper.
    /// </summary>
    /// <param name="transportType">Transport backend.</param>
    /// <param name="identifier">Transport identifier.</param>
    public NV120CLEDevice(TransportType transportType, string identifier)
        : base(transportType, identifier)
    {
        Knob = new NVCLEKnob(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [NVKnob.CmdMode] = "encmode",
            [NVKnob.CmdSampleTime] = "enctime",
            [NVKnob.CmdStepLimit] = "enclim",
            [NVKnob.CmdAccelExponent] = "encexp",
            [NVKnob.CmdStepOpenLoop] = "encstol",
            [NVCLEKnob.CmdStepClosedLoop] = "encstcl",
        });
    }

    /// <inheritdoc/>
    public override string? DeviceId => "NV120CLE";
    /// <inheritdoc/>
    protected override string NVFamilyIdentifier => "NV120CLE";
    /// <inheritdoc/>
    protected override int MaxChannelCount => 1;
    /// <inheritdoc/>
    protected override NVFamilyChannel CreateChannel(int channelId) => new NV120CLEChannel(channelId, WriteChannelAsync);

    /// <summary>
    /// Gets typed NV120CLE channels keyed by channel identifier.
    /// </summary>
    public new IReadOnlyDictionary<int, NV120CLEChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (NV120CLEChannel)kv.Value);
    /// <summary>
    /// Front-panel knob configuration capability including closed-loop step size.
    /// </summary>
    public NVCLEKnob Knob { get; }
}
