namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for selecting and querying monitor output source.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Monitor output is typically an analog 0-10V signal, scaling depends on device model and selected source, and available source enum members are device-specific.</para>
/// </remarks>
public class MonitorOutput(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, Type enumType)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for monitor output source selection.
    /// </summary>
    internal const string CmdOutputSrc = "MONITOR_OUTPUT_SOURCE";
    private readonly Type _enumType = enumType;

    /// <summary>
    /// Sets monitor output source.
    /// </summary>
    /// <param name="source">Enum value representing desired source.</param>
    /// <remarks>
    /// <para><b>Notes:</b> The source value must belong to the enum type supplied to the constructor; output routing updates in real time on supporting hardware.</para>
    /// </remarks>
    public virtual async Task SetAsync(Enum source)
        => _ = await WriteAsync(CmdOutputSrc, [source]).ConfigureAwait(false);

    /// <summary>
    /// Reads monitor output source.
    /// </summary>
    /// <returns>Enum value typed to the enum passed into constructor.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> If the device returns a value that is not defined in the configured enum type, this method falls back to enum value 0.</para>
    /// </remarks>
    public virtual async Task<Enum> GetAsync()
    {
        var value = int.Parse((await WriteAsync(CmdOutputSrc).ConfigureAwait(false))[0]);
        return Enum.IsDefined(_enumType, value)
            ? (Enum)Enum.ToObject(_enumType, value)
            : (Enum)Enum.ToObject(_enumType, 0);
    }
}
