using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.DDriveFamily;

/// <summary>
/// Concrete implementation for PSJ 30DV single-channel devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> PSJ 30DV devices use the d-Drive family command set but expose exactly one logical channel (index 0).</para>
/// </remarks>
public sealed class PSJ30DVDevice(TransportType transportType, string identifier)
    : DDriveFamilyDevice(transportType, identifier)
{
    static PSJ30DVDevice()
    {
        DeviceModelRegistry.Registry["30DV50/300"] = static (transport, id) => new PSJ30DVDevice(transport, id);
    }

    /// <inheritdoc/>
    public override string? DeviceId => "30DV50/300";
    /// <inheritdoc/>
    internal override string DDriveIdentifier => "AP";

    /// <inheritdoc/>
    internal override Task DiscoverChannelsAsync()
    {
        ChannelsInternal.Clear();
        ChannelsInternal[0] = new PSJ30DVChannel(0, WriteChannelAsync);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends commands without channel suffix because PSJ30DV is single-channel.
    /// </summary>
    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Notes:</b> Channel IDs are intentionally omitted from protocol frames on this model.</para>
    /// </remarks>
    internal override Task<IReadOnlyList<string>> WriteChannelAsync(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null)
    {
        return base.WriteChannelAsync(null, cmd, parameters);
    }

    /// <summary>
    /// Gets typed PSJ30DV channel map (single channel at index 0).
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only key 0 is expected for valid hardware.</para>
    /// </remarks>
    public new IReadOnlyDictionary<int, PSJ30DVChannel> Channels => ChannelsInternal.ToDictionary(kv => kv.Key, kv => (PSJ30DVChannel)kv.Value);
}
