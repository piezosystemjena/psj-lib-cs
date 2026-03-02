using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

/// <summary>
/// d-Drive monitor output source selection.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> The monitor output is typically a 0-10V analog signal intended for diagnostics (for example oscilloscope-based tuning and error observation).</para>
/// </remarks>
public enum DDriveMonitorOutputSource
{
    /// <summary>
    /// Closed-loop measured position.
    /// </summary>
    ClosedLoopPosition = 0,
    /// <summary>
    /// Setpoint value.
    /// </summary>
    Setpoint = 1,
    /// <summary>
    /// Controller output voltage.
    /// </summary>
    ControllerVoltage = 2,
    /// <summary>
    /// Signed position error.
    /// </summary>
    PositionError = 3,
    /// <summary>
    /// Absolute position error.
    /// </summary>
    PositionErrorAbs = 4,
    /// <summary>
    /// Actuator voltage.
    /// </summary>
    ActuatorVoltage = 5,
    /// <summary>
    /// Open-loop position estimate.
    /// </summary>
    OpenLoopPosition = 6,
}
