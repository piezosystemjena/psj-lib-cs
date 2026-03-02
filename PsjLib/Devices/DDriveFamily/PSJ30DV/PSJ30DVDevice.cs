using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

public sealed class PSJ30DVDevice(TransportType transportType, string identifier)
    : DDriveFamilyDevice(transportType, identifier)
{
    static PSJ30DVDevice()
    {
        DeviceModelRegistry.Registry["30DV50/300"] = static (transport, id) => new PSJ30DVDevice(transport, id);
    }

    public override string? DeviceId => "30DV50/300";
    protected override string DDriveIdentifier => "AP";

    protected override Task DiscoverChannelsAsync()
    {
        ChannelsInternal.Clear();
        ChannelsInternal[0] = new PSJ30DVChannel(0, WriteChannelAsync);
        return Task.CompletedTask;
    }

    internal override Task<IReadOnlyList<string>> WriteChannelAsync(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null)
    {
        return base.WriteChannelAsync(null, cmd, parameters);
    }

    public new IReadOnlyDictionary<int, PSJ30DVChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (PSJ30DVChannel)kv.Value);
}
