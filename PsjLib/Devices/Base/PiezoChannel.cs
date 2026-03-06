namespace PsjLib.Base;

/// <summary>
/// Writes a command for a specific channel and returns parsed response values.
/// </summary>
/// <param name="channelId">Channel index or <see langword="null"/> for global commands.</param>
/// <param name="cmd">Device command string.</param>
/// <param name="parameters">Optional command parameters.</param>
/// <returns>List of parsed response fields.</returns>
public delegate Task<IReadOnlyList<string>> ChannelWriteCallback(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null);

/// <summary>
/// Base class for per-channel capability access.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Channel IDs and available capabilities are defined by concrete device families and may not be contiguous across all possible indices.</para>
/// </remarks>
public class PiezoChannel
{
    private readonly ChannelWriteCallback _writeCallback;

    /// <summary>
    /// Initializes a new channel wrapper.
    /// </summary>
    /// <param name="id">Channel identifier used by the device protocol.</param>
    /// <param name="writeCallback">Callback used to execute channel commands.</param>
    public PiezoChannel(int id, ChannelWriteCallback writeCallback)
    {
        Id = id;
        _writeCallback = writeCallback;
    }

    /// <summary>
    /// Gets the channel identifier.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets commands included when channel backup is requested.
    /// </summary>
    internal virtual ISet<string> BackupCommands { get; } = new HashSet<string>();

    /// <summary>
    /// Executes a command against this channel.
    /// </summary>
    /// <param name="cmd">Device command.</param>
    /// <param name="parameters">Optional arguments.</param>
    /// <returns>Parsed response fields.</returns>
    internal Task<IReadOnlyList<string>> WriteAsync(string cmd, IReadOnlyList<object?>? parameters = null)
        => _writeCallback(Id, cmd, parameters);

    /// <summary>
    /// Executes a capability command using a capability-to-device command map.
    /// </summary>
    /// <param name="deviceCommands">Mapping from capability command tokens to protocol commands.</param>
    /// <param name="cmd">Capability command token.</param>
    /// <param name="parameters">Optional command arguments.</param>
    /// <returns>Parsed response fields or an empty list when command is not mapped.</returns>
    internal async Task<IReadOnlyList<string>> CapabilityWriteAsync(
        IReadOnlyDictionary<string, string> deviceCommands,
        string cmd,
        IReadOnlyList<object?>? parameters = null)
    {
        if (!deviceCommands.TryGetValue(cmd, out var mappedCommand))
        {
            return [];
        }

        return await WriteAsync(mappedCommand, parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads all commands listed in <see cref="BackupCommands"/> and returns their values.
    /// </summary>
    /// <returns>Dictionary mapping command names to response payloads.</returns>
    public virtual async Task<Dictionary<string, IReadOnlyList<string>>> BackupAsync()
    {
        var backup = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var cmd in BackupCommands)
        {
            backup[cmd] = await WriteAsync(cmd).ConfigureAwait(false);
        }

        return backup;
    }
}
