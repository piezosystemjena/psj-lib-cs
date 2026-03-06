using System.Collections.Concurrent;

namespace PsjLib.Base;

/// <summary>
/// Stores cached command responses for read-only or cacheable device commands.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Cache entries are invalidated on writes to the corresponding base command; cross-process or external device changes are not automatically observed.</para>
/// </remarks>
public sealed class CommandCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _cache = new();
    private readonly HashSet<string> _cacheableCommands;

    /// <summary>
    /// Initializes a new command cache.
    /// </summary>
    /// <param name="cacheableCommands">Command names that are eligible for caching.</param>
    /// <param name="enabled">Initial cache state.</param>
    public CommandCache(IEnumerable<string> cacheableCommands, bool enabled = true)
    {
        _cacheableCommands = [.. cacheableCommands];
        Enabled = enabled;
    }

    /// <summary>
    /// Gets or sets whether caching is active.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Determines whether a command is cacheable.
    /// </summary>
    /// <param name="cmd">Raw command string, optionally with arguments.</param>
    /// <returns><see langword="true"/> when command or base command is configured as cacheable.</returns>
    public bool IsCacheable(string cmd)
    {
        if (_cacheableCommands.Contains(cmd))
        {
            return true;
        }

        var baseCmd = cmd.Split(',')[0];
        return _cacheableCommands.Contains(baseCmd);
    }

    /// <summary>
    /// Gets a cached command response if caching is enabled and an entry exists.
    /// </summary>
    /// <param name="cmd">Command key.</param>
    /// <returns>Cached response values, or <see langword="null"/> when no cache hit exists.</returns>
    public IReadOnlyList<string>? Get(string cmd)
    {
        if (!Enabled)
        {
            return null;
        }

        return _cache.GetValueOrDefault(cmd);
    }

    /// <summary>
    /// Stores a response for a command when caching is enabled and allowed.
    /// </summary>
    /// <param name="cmd">Command key.</param>
    /// <param name="values">Response values to cache.</param>
    public void Set(string cmd, IReadOnlyList<string> values)
    {
        if (!Enabled || !IsCacheable(cmd))
        {
            return;
        }

        _cache[cmd] = values.ToList();
    }

    /// <summary>
    /// Removes a single command entry from the cache.
    /// </summary>
    /// <param name="cmd">Command key.</param>
    public void Invalidate(string cmd) => _cache.TryRemove(cmd, out _);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    public void Clear() => _cache.Clear();
}
