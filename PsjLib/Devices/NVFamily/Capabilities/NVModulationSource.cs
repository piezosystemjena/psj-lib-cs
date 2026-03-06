using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV-family modulation source options.
/// </summary>
public enum NVModulationSourceTypes
{
    /// <summary>
    /// Front-panel encoder + analog control path.
    /// </summary>
    EncoderAnalog = 0,
    /// <summary>
    /// Serial command control path.
    /// </summary>
    Serial = 1,
}

/// <summary>
/// NV-family modulation source capability with client-side readback cache.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> NV devices do not provide modulation source readback; <see cref="GetAsync"/> returns the value last set via this client.</para>
/// </remarks>
public sealed class NVModulationSource(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : ModulationSource(writeCb, commands, typeof(NVModulationSourceTypes))
{
    private NVModulationSourceTypes _sourceCache = NVModulationSourceTypes.EncoderAnalog;

    /// <inheritdoc/>
    public override async Task SetAsync(Enum source)
    {
        if (source is not NVModulationSourceTypes typed)
        {
            throw new ArgumentException($"Invalid modulation source type: {source.GetType()} (Expected: {nameof(NVModulationSourceTypes)})", nameof(source));
        }

        await base.SetAsync(typed).ConfigureAwait(false);
        _sourceCache = typed;
    }

    /// <inheritdoc/>
    public override Task<Enum> GetAsync()
    {
        return Task.FromResult<Enum>(_sourceCache);
    }
}
