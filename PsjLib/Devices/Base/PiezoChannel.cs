namespace PsjLib.Base;

public delegate Task<IReadOnlyList<string>> ChannelWriteCallback(int? channelId, string cmd, IReadOnlyList<object?>? parameters = null);

public class PiezoChannel
{
    private readonly ChannelWriteCallback _writeCallback;

    public PiezoChannel(int id, ChannelWriteCallback writeCallback)
    {
        Id = id;
        _writeCallback = writeCallback;
    }

    public int Id { get; }
    public virtual ISet<string> BackupCommands { get; } = new HashSet<string>();

    protected internal Task<IReadOnlyList<string>> WriteAsync(string cmd, IReadOnlyList<object?>? parameters = null)
        => _writeCallback(Id, cmd, parameters);

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
