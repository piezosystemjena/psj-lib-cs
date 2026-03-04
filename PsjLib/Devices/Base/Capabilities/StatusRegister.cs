namespace PsjLib.Base.Capabilities;

/// <summary>
/// Base class for parsed status register wrappers.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Concrete device families decode raw status payloads into model-specific fields.</para>
/// </remarks>
/// <param name="rawValues">Raw status fields returned by the device.</param>
public class StatusRegister
{
    /// <summary>
    /// Initializes a new status-register wrapper.
    /// </summary>
    /// <param name="rawValues">Raw status fields returned by the device.</param>
    /// <param name="channelId">Optional channel context for channel-indexed status payloads.</param>
    protected StatusRegister(IReadOnlyList<string> rawValues, int? channelId = null)
    {
        RawValues = rawValues;
        ChannelId = channelId;
    }

    /// <summary>
    /// Gets the raw status fields that derived classes decode.
    /// </summary>
    protected IReadOnlyList<string> RawValues { get; }

    /// <summary>
    /// Gets optional channel context for channel-indexed status payloads.
    /// </summary>
    protected int? ChannelId { get; }
}
