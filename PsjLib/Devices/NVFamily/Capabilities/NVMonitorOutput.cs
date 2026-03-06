using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV-family monitor output routing sources.
/// </summary>
public enum NVMonitorOutputSource
{
    /// <summary>
    /// Unknown or unmapped source.
    /// </summary>
    Unknown = -1,
    /// <summary>
    /// Actuator voltage signal.
    /// </summary>
    ActuatorVoltage = 0,
    /// <summary>
    /// Position-related voltage signal.
    /// </summary>
    PositionVoltage = 1,
    /// <summary>
    /// Mode-dependent source. Voltage in open loop, position in closed-loop modes.
    /// </summary>
    ModeDependent = 2,
}

/// <summary>
/// NV-family monitor output capability with client-side readback cache.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> NV devices do not provide monitor source readback; <see cref="GetAsync"/> returns the value last set via this client.</para>
/// </remarks>
public sealed class NVMonitorOutput(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : MonitorOutput(writeCb, commands, typeof(NVMonitorOutputSource))
{
    private NVMonitorOutputSource _sourceCache = NVMonitorOutputSource.Unknown;

    /// <inheritdoc/>
    public override async Task SetAsync(Enum source)
    {
        if (source is not NVMonitorOutputSource typed)
        {
            throw new ArgumentException($"Invalid monitor source type: {source.GetType()} (Expected: {nameof(NVMonitorOutputSource)})", nameof(source));
        }

        _ = await WriteAsync(CmdOutputSrc, [typed]).ConfigureAwait(false);
        _sourceCache = typed;
    }

    /// <inheritdoc/>
    public override Task<Enum> GetAsync()
    {
        return Task.FromResult<Enum>(_sourceCache);
    }
}
