namespace PsjLib.Base.Capabilities;

/// <summary>
/// Capability for configuring PID controller gains and filter constant.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> PID gain semantics, units, and safe ranges are device-specific; incorrect tuning can destabilize motion and should be adjusted conservatively.</para>
/// </remarks>
public class PIDController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>
    /// Command token for proportional gain.
    /// </summary>
    public const string CmdP = "PID_P";
    /// <summary>
    /// Command token for integral gain.
    /// </summary>
    public const string CmdI = "PID_I";
    /// <summary>
    /// Command token for derivative gain.
    /// </summary>
    public const string CmdD = "PID_D";
    /// <summary>
    /// Command token for derivative filter time constant.
    /// </summary>
    public const string CmdTf = "PID_TF";

    /// <summary>
    /// Updates one or more PID parameters.
    /// </summary>
    /// <remarks>
    /// <para><b>Notes:</b> Only non-null parameters are written; coordinate gain changes with system testing to avoid oscillation and noise amplification.</para>
    /// </remarks>
    /// <param name="p">Optional proportional gain.</param>
    /// <param name="i">Optional integral gain.</param>
    /// <param name="d">Optional derivative gain.</param>
    /// <param name="tf">Optional derivative filter time constant.</param>
    public virtual async Task SetAsync(double? p = null, double? i = null, double? d = null, double? tf = null)
    {
        if (p is not null)
            _ = await WriteAsync(CmdP, [p.Value]).ConfigureAwait(false);
        if (i is not null)
            _ = await WriteAsync(CmdI, [i.Value]).ConfigureAwait(false);
        if (d is not null)
            _ = await WriteAsync(CmdD, [d.Value]).ConfigureAwait(false);
        if (tf is not null)
            _ = await WriteAsync(CmdTf, [tf.Value]).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads all PID parameters.
    /// </summary>
    /// <returns>Tuple containing <c>P</c>, <c>I</c>, <c>D</c>, and <c>Tf</c>.</returns>
    public virtual async Task<(double P, double I, double D, double Tf)> GetAsync()
    {
        var p = double.Parse((await WriteAsync(CmdP).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var i = double.Parse((await WriteAsync(CmdI).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var d = double.Parse((await WriteAsync(CmdD).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var tf = double.Parse((await WriteAsync(CmdTf).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        return (p, i, d, tf);
    }

    /// <summary>
    /// Reads proportional gain.
    /// </summary>
    /// <returns>Proportional gain value.</returns>
    public virtual async Task<double> GetPAsync()
        => double.Parse((await WriteAsync(CmdP).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads integral gain.
    /// </summary>
    /// <returns>Integral gain value.</returns>
    public virtual async Task<double> GetIAsync()
        => double.Parse((await WriteAsync(CmdI).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads derivative gain.
    /// </summary>
    /// <returns>Derivative gain value.</returns>
    public virtual async Task<double> GetDAsync()
        => double.Parse((await WriteAsync(CmdD).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads derivative filter time constant.
    /// </summary>
    /// <returns>Filter time constant value.</returns>
    public virtual async Task<double> GetTfAsync()
        => double.Parse((await WriteAsync(CmdTf).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
