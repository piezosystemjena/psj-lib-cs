namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for reading textual device report output.
/// </summary>
public sealed class Report(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for report retrieval.
    /// </summary>
    internal const string CmdReport = "REPORT";

    /// <summary>
    /// Reads a report string from firmware.
    /// </summary>
    /// <returns>Report text.</returns>
    public async Task<string> GetAsync() => (await WriteAsync(CmdReport).ConfigureAwait(false))[0];
}
