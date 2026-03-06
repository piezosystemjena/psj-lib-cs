namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for reading and constructing typed status register objects.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Status payload layout is device-specific; this type materializes responses into the configured status-register type.</para>
/// </remarks>
/// <typeparam name="TStatusRegister">Concrete status register type with a constructor taking raw response values.</typeparam>
public sealed class Status<TStatusRegister>(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands, int? channelId = null)
    : PiezoCapability(writeCb, commands)
    where TStatusRegister : StatusRegister
{
    /// <summary>
    /// Command token for status register read.
    /// </summary>
    internal const string CmdStatus = "STATUS";

    /// <summary>
    /// Reads status payload and constructs a typed status register.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Returned interpretation depends on the selected status-register implementation and model-specific status encoding.</para>
    /// </remarks>
    /// <returns>Typed status register instance.</returns>
    public async Task<TStatusRegister> GetAsync()
    {
        var raw = await WriteAsync(CmdStatus).ConfigureAwait(false);
        var ctor = typeof(TStatusRegister).GetConstructor([typeof(IReadOnlyList<string>), typeof(int?)]);
        if (ctor is not null)
        {
            return (TStatusRegister)ctor.Invoke([raw, channelId]);
        }

        return (TStatusRegister)Activator.CreateInstance(typeof(TStatusRegister), raw)!;
    }
}
