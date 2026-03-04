namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for selecting and querying modulation source.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Source enums are device-specific and unknown values returned by the device map to enum value 0 when no explicit member exists.</para>
/// </remarks>
public class ModulationSource(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, Type enumType)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for modulation source selection.
    /// </summary>
    public const string CmdSource = "MODULATION_SOURCE";
    private readonly Type _enumType = enumType;

    /// <summary>
    /// Sets modulation source.
    /// </summary>
    /// <param name="source">Enum value representing desired modulation source.</param>
    /// <remarks>
    /// <para><b>Notes:</b> The provided enum should match the device-specific source type configured for this capability instance.</para>
    /// </remarks>
    public virtual async Task SetAsync(Enum source)
        => _ = await WriteAsync(CmdSource, [source]).ConfigureAwait(false);

    /// <summary>
    /// Reads modulation source.
    /// </summary>
    /// <returns>Enum value typed to the enum passed into constructor.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Unrecognized device values are mapped to enum value 0 when the value is not defined in the supplied enum type.</para>
    /// </remarks>
    public virtual async Task<Enum> GetAsync()
    {
        var value = int.Parse((await WriteAsync(CmdSource).ConfigureAwait(false))[0]);
        return Enum.IsDefined(_enumType, value)
            ? (Enum)Enum.ToObject(_enumType, value)
            : (Enum)Enum.ToObject(_enumType, 0);
    }
}
