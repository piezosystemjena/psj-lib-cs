namespace PsjLib.Base.Capabilities;

public class PIDController(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdP = "PID_P";
    public const string CmdI = "PID_I";
    public const string CmdD = "PID_D";
    public const string CmdTf = "PID_TF";

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

    public virtual async Task<(double P, double I, double D, double Tf)> GetAsync()
    {
        var p = double.Parse((await WriteAsync(CmdP).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var i = double.Parse((await WriteAsync(CmdI).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var d = double.Parse((await WriteAsync(CmdD).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        var tf = double.Parse((await WriteAsync(CmdTf).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
        return (p, i, d, tf);
    }

    public virtual async Task<double> GetPAsync()
        => double.Parse((await WriteAsync(CmdP).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    public virtual async Task<double> GetIAsync()
        => double.Parse((await WriteAsync(CmdI).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    public virtual async Task<double> GetDAsync()
        => double.Parse((await WriteAsync(CmdD).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);

    public virtual async Task<double> GetTfAsync()
        => double.Parse((await WriteAsync(CmdTf).ConfigureAwait(false))[0], System.Globalization.CultureInfo.InvariantCulture);
}
