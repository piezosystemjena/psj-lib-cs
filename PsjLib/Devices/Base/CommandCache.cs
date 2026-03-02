namespace PsjLib.Base;

public sealed class CommandCache
{
    private readonly Dictionary<string, IReadOnlyList<string>> _cache = new();
    private readonly HashSet<string> _cacheableCommands;

    public CommandCache(IEnumerable<string> cacheableCommands, bool enabled = true)
    {
        _cacheableCommands = [.. cacheableCommands];
        Enabled = enabled;
    }

    public bool Enabled { get; set; } = true;

    public bool IsCacheable(string cmd)
    {
        if (_cacheableCommands.Contains(cmd))
        {
            return true;
        }

        var baseCmd = cmd.Split(',')[0];
        return _cacheableCommands.Contains(baseCmd);
    }

    public IReadOnlyList<string>? Get(string cmd)
    {
        if (!Enabled)
        {
            return null;
        }

        return _cache.GetValueOrDefault(cmd);
    }

    public void Set(string cmd, IReadOnlyList<string> values)
    {
        if (!Enabled || !IsCacheable(cmd))
        {
            return;
        }

        _cache[cmd] = values.ToList();
    }

    public void Invalidate(string cmd) => _cache.Remove(cmd);
    public void Clear() => _cache.Clear();
}
