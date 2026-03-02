namespace PsjLib.Base.Capabilities;

public class StatusRegister(IReadOnlyList<string> rawValues)
{
    protected IReadOnlyList<string> RawValues { get; } = rawValues;
}
