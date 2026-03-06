namespace PsjLib.Base;

/// <summary>
/// Base exception for device-level protocol and command errors.
/// </summary>
public class DeviceError : Exception
{
    /// <summary>
    /// Initializes a new <see cref="DeviceError"/> instance.
    /// </summary>
    /// <param name="message">Human-readable error details.</param>
    public DeviceError(string message) : base(message) { }
}

/// <summary>
/// Represents an unspecified or unmapped device error.
/// </summary>
public sealed class ErrorNotSpecified : DeviceError { public ErrorNotSpecified(string message) : base(message) { } }
/// <summary>
/// Raised when a command is not recognized by the device firmware.
/// </summary>
public sealed class UnknownCommand : DeviceError { public UnknownCommand(string message) : base(message) { } }
/// <summary>
/// Raised when required command arguments are missing.
/// </summary>
public sealed class ParameterMissing : DeviceError { public ParameterMissing(string message) : base(message) { } }
/// <summary>
/// Raised when at least one command parameter is outside admissible range.
/// </summary>
public sealed class AdmissibleParameterRangeExceeded : DeviceError { public AdmissibleParameterRangeExceeded(string message) : base(message) { } }
/// <summary>
/// Raised when more parameters are provided than supported by the command.
/// </summary>
public sealed class CommandParameterCountExceeded : DeviceError { public CommandParameterCountExceeded(string message) : base(message) { } }
/// <summary>
/// Raised when writing a locked or read-only parameter.
/// </summary>
public sealed class ParameterLockedOrReadOnly : DeviceError { public ParameterLockedOrReadOnly(string message) : base(message) { } }
/// <summary>
/// Raised when control loop or actuator is under-driven.
/// </summary>
public sealed class Underload : DeviceError { public Underload(string message) : base(message) { } }
/// <summary>
/// Raised when control loop or actuator is overloaded.
/// </summary>
public sealed class Overload : DeviceError { public Overload(string message) : base(message) { } }
/// <summary>
/// Raised when a parameter is below the minimum allowed value.
/// </summary>
public sealed class ParameterTooLow : DeviceError { public ParameterTooLow(string message) : base(message) { } }
/// <summary>
/// Raised when a parameter is above the maximum allowed value.
/// </summary>
public sealed class ParameterTooHigh : DeviceError { public ParameterTooHigh(string message) : base(message) { } }
/// <summary>
/// Raised when the requested channel does not exist on the device.
/// </summary>
public sealed class UnknownChannel : DeviceError { public UnknownChannel(string message) : base(message) { } }
/// <summary>
/// Raised when no actuator is connected or recognized for the channel.
/// </summary>
public sealed class ActuatorNotConnected : DeviceError { public ActuatorNotConnected(string message) : base(message) { } }

/// <summary>
/// Numeric error codes reported by the device firmware.
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Unknown or unspecified error.
    /// </summary>
    ErrorNotSpecified = 1,
    /// <summary>
    /// Command is not known by the firmware.
    /// </summary>
    UnknownCommand = 2,
    /// <summary>
    /// One or more required parameters are missing.
    /// </summary>
    ParameterMissing = 3,
    /// <summary>
    /// Provided parameter is out of admissible range.
    /// </summary>
    AdmissibleParameterRangeExceeded = 4,
    /// <summary>
    /// Too many command parameters were supplied.
    /// </summary>
    CommandParameterCountExceeded = 5,
    /// <summary>
    /// Parameter is locked or read-only.
    /// </summary>
    ParameterLockedOrReadOnly = 6,
    /// <summary>
    /// Device reports underload condition.
    /// </summary>
    Underload = 7,
    /// <summary>
    /// Device reports overload condition.
    /// </summary>
    Overload = 8,
    /// <summary>
    /// Parameter is below the supported minimum.
    /// </summary>
    ParameterTooLow = 9,
    /// <summary>
    /// Parameter is above the supported maximum.
    /// </summary>
    ParameterTooHigh = 10,
    /// <summary>
    /// No compatible actuator is connected.
    /// </summary>
    ActuatorNotConnected = 98,
    /// <summary>
    /// Referenced channel does not exist.
    /// </summary>
    UnknownChannel = 99,
}

/// <summary>
/// Extension helpers for converting numeric error codes into typed exceptions.
/// </summary>
public static class ErrorCodeExtensions
{
    /// <summary>
    /// Throws a concrete <see cref="DeviceError"/> that corresponds to the specified <paramref name="code"/>.
    /// </summary>
    /// <param name="code">Firmware error code to map.</param>
    /// <param name="message">Optional override message. If <see langword="null"/>, the enum name is used.</param>
    /// <exception cref="DeviceError">Always thrown as a concrete derived type.</exception>
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
