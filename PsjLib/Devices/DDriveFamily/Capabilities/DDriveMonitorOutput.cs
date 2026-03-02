using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.DDriveFamily.Capabilities;

public enum DDriveMonitorOutputSource
{
    ClosedLoopPosition = 0,
    Setpoint = 1,
    ControllerVoltage = 2,
    PositionError = 3,
    PositionErrorAbs = 4,
    ActuatorVoltage = 5,
    OpenLoopPosition = 6,
}
