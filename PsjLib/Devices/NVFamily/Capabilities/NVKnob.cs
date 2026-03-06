using PsjLib.Base.Capabilities;

namespace PsjLib.NVFamily.Capabilities;

/// <summary>
/// NV encoder knob operating modes.
/// </summary>
public enum NVKnobMode
{
    /// <summary>
    /// Unknown or unmapped mode.
    /// </summary>
    Unknown = -1,
    /// <summary>
    /// Step size depends on rotation speed.
    /// </summary>
    Acceleration = 0,
    /// <summary>
    /// Fixed interval mode.
    /// </summary>
    Interval = 1,
    /// <summary>
    /// Interval mode with acceleration behavior.
    /// </summary>
    IntervalAcceleration = 2,
}

/// <summary>
/// Capability for configuring NV-series front-panel encoder knob behavior.
/// </summary>
public class NVKnob(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    /// <summary>Command token for knob mode.</summary>
    internal const string CmdMode = "KNOB_MODE";
    /// <summary>Command token for knob sampling time.</summary>
    internal const string CmdSampleTime = "KNOB_SAMPLE_TIME";
    /// <summary>Command token for acceleration exponent.</summary>
    internal const string CmdAccelExponent = "KNOB_ACCEL_EXPONENT";
    /// <summary>Command token for step limit.</summary>
    internal const string CmdStepLimit = "KNOB_STEP_LIMIT";
    /// <summary>Command token for open-loop step size.</summary>
    internal const string CmdStepOpenLoop = "KNOB_STEP_OPEN_LOOP";

    /// <summary>
    /// Sets encoder knob parameters; only non-null values are written.
    /// </summary>
    public virtual async Task SetAsync(
        NVKnobMode? mode = null,
        double? sampleTime = null,
        int? accelExponent = null,
        int? stepLimit = null,
        double? stepOpenLoop = null)
    {
        if (mode is not null)
        {
            _ = await WriteAsync(CmdMode, [mode.Value]).ConfigureAwait(false);
        }

        if (sampleTime is not null)
        {
            var output = (int)(sampleTime.Value / 0.02);
            _ = await WriteAsync(CmdSampleTime, [output]).ConfigureAwait(false);
        }

        if (accelExponent is not null)
        {
            _ = await WriteAsync(CmdAccelExponent, [accelExponent.Value]).ConfigureAwait(false);
        }

        if (stepLimit is not null)
        {
            _ = await WriteAsync(CmdStepLimit, [stepLimit.Value]).ConfigureAwait(false);
        }

        if (stepOpenLoop is not null)
        {
            _ = await WriteAsync(CmdStepOpenLoop, [stepOpenLoop.Value]).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reads current knob mode.
    /// </summary>
    public async Task<NVKnobMode> GetModeAsync()
    {
        var result = await WriteAsync(CmdMode).ConfigureAwait(false);
        return int.TryParse(result[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var mode)
            && Enum.IsDefined(typeof(NVKnobMode), mode)
            ? (NVKnobMode)mode
            : NVKnobMode.Unknown;
    }

    /// <summary>
    /// Reads knob sample time in seconds.
    /// </summary>
    public async Task<double> GetSampleTimeAsync()
    {
        var result = await WriteAsync(CmdSampleTime).ConfigureAwait(false);
        var sampleUnits = int.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
        return sampleUnits * 0.02;
    }

    /// <summary>
    /// Reads configured acceleration exponent.
    /// </summary>
    public async Task<int> GetAccelExponentAsync()
    {
        var result = await WriteAsync(CmdAccelExponent).ConfigureAwait(false);
        return int.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Reads configured step limit.
    /// </summary>
    public async Task<int> GetStepLimitAsync()
    {
        var result = await WriteAsync(CmdStepLimit).ConfigureAwait(false);
        return int.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Reads configured open-loop step size.
    /// </summary>
    public async Task<double> GetStepOpenLoopAsync()
    {
        var result = await WriteAsync(CmdStepOpenLoop).ConfigureAwait(false);
        return double.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// NV knob capability variant that includes closed-loop step configuration.
/// </summary>
public sealed class NVCLEKnob(CapabilityWriteCallback writeCb, IReadOnlyDictionary<string, string> commands)
    : NVKnob(writeCb, commands)
{
    /// <summary>
    /// Command token for closed-loop step size.
    /// </summary>
    internal const string CmdStepClosedLoop = "KNOB_STEP_CLOSED_LOOP";

    /// <summary>
    /// Sets encoder knob parameters including optional closed-loop step size.
    /// </summary>
    public async Task SetAsync(
        NVKnobMode? mode = null,
        double? sampleTime = null,
        int? accelExponent = null,
        int? stepLimit = null,
        double? stepOpenLoop = null,
        double? stepClosedLoop = null)
    {
        await base.SetAsync(mode, sampleTime, accelExponent, stepLimit, stepOpenLoop).ConfigureAwait(false);

        if (stepClosedLoop is not null)
        {
            _ = await WriteAsync(CmdStepClosedLoop, [stepClosedLoop.Value]).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reads configured closed-loop step size.
    /// </summary>
    public async Task<double> GetStepClosedLoopAsync()
    {
        var result = await WriteAsync(CmdStepClosedLoop).ConfigureAwait(false);
        return double.Parse(result[0], System.Globalization.CultureInfo.InvariantCulture);
    }
}
