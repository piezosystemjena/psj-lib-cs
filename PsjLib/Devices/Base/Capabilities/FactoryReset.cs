namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability that triggers device factory reset.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Factory reset discards custom configuration; back up settings before use when configuration recovery is required.</para>
/// </remarks>
public sealed class FactoryReset(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for executing factory reset.
    /// </summary>
    public const string CmdReset = "FACTORY_RESET";

    /// <summary>
    /// Executes a factory reset command.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> This operation is not reversible through the API.</para>
    /// </remarks>
    public async Task ExecuteAsync() => _ = await WriteAsync(CmdReset).ConfigureAwait(false);
}
