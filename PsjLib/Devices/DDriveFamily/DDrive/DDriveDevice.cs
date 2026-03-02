using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

public sealed class DDriveDevice(TransportType transportType, string identifier)
    : DDriveFamilyDevice(transportType, identifier)
{
    static DDriveDevice()
    {
        DeviceModelRegistry.Registry["d-Drive"] = static (transport, id) => new DDriveDevice(transport, id);
    }

    public override string? DeviceId => "d-Drive";
    protected override string DDriveIdentifier => "DSM";

    protected override async Task DiscoverChannelsAsync()
    {
        var response = await WriteRawAsync("stat").ConfigureAwait(false);
        ParseChannelStatus(response);

        foreach (var channel in Channels.Values)
        {
            _ = await WriteChannelAsync(channel.Id, "setf", [1]).ConfigureAwait(false);
        }
    }

    private void ParseChannelStatus(string response)
    {
        ChannelsInternal.Clear();
        foreach (var line in response.Split('\n'))
        {
            var idx = line.IndexOf("stat,", StringComparison.OrdinalIgnoreCase);
            if (idx < 0 || idx + 5 >= line.Length)
            {
                continue;
            }

            if (!int.TryParse(line.Substring(idx + 5, 1), out var channelNumber))
            {
                continue;
            }

            ChannelsInternal[channelNumber] = new DDriveChannel(channelNumber, WriteChannelAsync);
        }
    }

    public new IReadOnlyDictionary<int, DDriveChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (DDriveChannel)kv.Value);
}
