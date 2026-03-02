namespace PsjLib.Base;

public class DeviceError : Exception
{
    public DeviceError(string message) : base(message) { }
}

public sealed class ErrorNotSpecified : DeviceError { public ErrorNotSpecified(string message) : base(message) { } }
public sealed class UnknownCommand : DeviceError { public UnknownCommand(string message) : base(message) { } }
public sealed class ParameterMissing : DeviceError { public ParameterMissing(string message) : base(message) { } }
public sealed class AdmissibleParameterRangeExceeded : DeviceError { public AdmissibleParameterRangeExceeded(string message) : base(message) { } }
public sealed class CommandParameterCountExceeded : DeviceError { public CommandParameterCountExceeded(string message) : base(message) { } }
public sealed class ParameterLockedOrReadOnly : DeviceError { public ParameterLockedOrReadOnly(string message) : base(message) { } }
public sealed class Underload : DeviceError { public Underload(string message) : base(message) { } }
public sealed class Overload : DeviceError { public Overload(string message) : base(message) { } }
public sealed class ParameterTooLow : DeviceError { public ParameterTooLow(string message) : base(message) { } }
public sealed class ParameterTooHigh : DeviceError { public ParameterTooHigh(string message) : base(message) { } }
public sealed class UnknownChannel : DeviceError { public UnknownChannel(string message) : base(message) { } }
public sealed class ActuatorNotConnected : DeviceError { public ActuatorNotConnected(string message) : base(message) { } }

public enum ErrorCode
{
    ErrorNotSpecified = 1,
    UnknownCommand = 2,
    ParameterMissing = 3,
    AdmissibleParameterRangeExceeded = 4,
    CommandParameterCountExceeded = 5,
    ParameterLockedOrReadOnly = 6,
    Underload = 7,
    Overload = 8,
    ParameterTooLow = 9,
    ParameterTooHigh = 10,
    ActuatorNotConnected = 98,
    UnknownChannel = 99,
}

public static class ErrorCodeExtensions
{
    public static void RaiseError(this ErrorCode code, string? message = null)
    {
        var msg = message ?? code.ToString();
        throw code switch
        {
            ErrorCode.UnknownCommand => new UnknownCommand(msg),
            ErrorCode.ParameterMissing => new ParameterMissing(msg),
            ErrorCode.AdmissibleParameterRangeExceeded => new AdmissibleParameterRangeExceeded(msg),
            ErrorCode.CommandParameterCountExceeded => new CommandParameterCountExceeded(msg),
            ErrorCode.ParameterLockedOrReadOnly => new ParameterLockedOrReadOnly(msg),
            ErrorCode.Underload => new Underload(msg),
            ErrorCode.Overload => new Overload(msg),
            ErrorCode.ParameterTooLow => new ParameterTooLow(msg),
            ErrorCode.ParameterTooHigh => new ParameterTooHigh(msg),
            ErrorCode.ActuatorNotConnected => new ActuatorNotConnected(msg),
            ErrorCode.UnknownChannel => new UnknownChannel(msg),
            _ => new ErrorNotSpecified(msg),
        };
    }
}
