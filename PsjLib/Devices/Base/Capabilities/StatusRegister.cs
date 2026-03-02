namespace PsjLib.Base.Capabilities;

/// <summary>
/// Base class for parsed status register wrappers.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Concrete device families decode raw status payloads into model-specific fields.</para>
/// </remarks>
/// <param name="rawValues">Raw status fields returned by the device.</param>
public class StatusRegister(IReadOnlyList<string> rawValues)
{
    /// <summary>
    /// Gets the raw status fields that derived classes decode.
    /// </summary>
    protected IReadOnlyList<string> RawValues { get; } = rawValues;
}
