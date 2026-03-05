using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Concrete implementation for d-Drive multi-channel devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Hardware may expose between one and six channels depending on module population.</para>
/// </remarks>
public sealed class DDriveDevice(TransportType transportType, string identifier)
    : DDriveFamilyDevice(transportType, identifier)
{
    static DDriveDevice()
    {
        DeviceModelRegistry.Registry["d-Drive"] = static (transport, id) => new DDriveDevice(transport, id);
    }

    /// <inheritdoc/>
    public override string? DeviceId => "d-Drive";
    /// <inheritdoc/>
    internal override string DDriveIdentifier => "DSM";

    /// <inheritdoc/>
    internal override async Task DiscoverChannelsAsync()
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

    /// <summary>
    /// Gets typed d-Drive channels keyed by channel identifier.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only discovered channel slots are present; callers should not assume contiguous keys.</para>
    /// </remarks>
    public new IReadOnlyDictionary<int, DDriveChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (DDriveChannel)kv.Value);
}
