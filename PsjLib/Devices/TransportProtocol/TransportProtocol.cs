using System.Text;

namespace PsjLib.Transport;

/// <summary>
/// Probes a connected transport and returns a detected device identifier.
/// </summary>
/// <param name="transport">Connected transport instance.</param>
/// <returns>Detected model identifier, or <see langword="null"/> if probe does not match.</returns>
public delegate Task<string?> DiscoveryCallback(TransportProtocol transport);

/// <summary>
/// Abstract base class for serial and network transport implementations.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Implementations are expected to preserve command/response framing and delimiter semantics consistently across transports.</para>
/// </remarks>
public abstract class TransportProtocol
{
    /// <summary>
    /// XON flow-control control byte.
    /// </summary>
    public static readonly byte[] Xon = [0x11];
    /// <summary>
    /// XOFF flow-control control byte.
    /// </summary>
    public static readonly byte[] Xoff = [0x13];
    /// <summary>
    /// Line feed delimiter.
    /// </summary>
    public static readonly byte[] Lf = [0x0A];
    /// <summary>
    /// Carriage return delimiter.
    /// </summary>
    public static readonly byte[] Cr = [0x0D];
    /// <summary>
    /// CRLF delimiter.
    /// </summary>
    public static readonly byte[] Crlf = [0x0D, 0x0A];

    /// <summary>
    /// Default operation timeout in seconds.
    /// </summary>
    public const double DefaultTimeoutSecs = 0.6;

    /// <summary>
    /// Gets or sets the delimiter used by <see cref="ReadMessageAsync(double)"/>.
    /// </summary>
    public byte[] RxDelimiter { get; set; } = Xon;

    /// <summary>
    /// Gets the transport backend type.
    /// </summary>
    public abstract TransportType TransportType { get; }
    /// <summary>
    /// Gets whether the underlying connection is currently open.
    /// </summary>
    public abstract bool IsConnected { get; }
    /// <summary>
    /// Gets the endpoint identifier used by this transport.
    /// </summary>
    public abstract string Identifier { get; }

    /// <summary>
    /// Discovers reachable endpoints for this transport.
    /// </summary>
    /// <param name="discoveryCallback">Callback used to probe connected candidates for device identity.</param>
    /// <returns>Detected endpoint list.</returns>
    public abstract Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(DiscoveryCallback discoveryCallback);
    /// <summary>
    /// Opens the underlying transport connection.
    /// </summary>
    /// <param name="autoAdjustCommParams">When <see langword="true"/>, implementation may tune communication settings.</param>
    public abstract Task ConnectAsync(bool autoAdjustCommParams = true);
    /// <summary>
    /// Discards pending bytes from the transport receive buffer.
    /// </summary>
    public abstract Task FlushInputAsync();
    /// <summary>
    /// Sends a raw command frame to the transport.
    /// </summary>
    /// <param name="cmd">Serialized command frame.</param>
    public abstract Task WriteAsync(string cmd);
    /// <summary>
    /// Reads from transport until <paramref name="expected"/> delimiter is observed.
    /// </summary>
    /// <param name="expected">Expected byte sequence delimiter.</param>
    /// <param name="timeoutSecs">Read timeout in seconds.</param>
    /// <returns>Response payload without delimiter bytes.</returns>
    public abstract Task<string> ReadUntilAsync(byte[] expected, double timeoutSecs = DefaultTimeoutSecs);
    /// <summary>
    /// Gets metadata for the current transport endpoint.
    /// </summary>
    /// <returns>Transport information snapshot.</returns>
    public abstract TransportProtocolInfo GetInfo();
    /// <summary>
    /// Closes the underlying transport and releases resources.
    /// </summary>
    public abstract Task CloseAsync();

    /// <summary>
    /// Sets an optional transport-specific property.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    public virtual void SetProperty(string name, object value)
    {
    }

    /// <summary>
    /// Gets an optional transport-specific property.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <returns>Property value or <see langword="null"/> if unsupported.</returns>
    public virtual object? GetProperty(string name)
    {
        return null;
    }

    /// <summary>
    /// Reads one complete protocol message using <see cref="RxDelimiter"/>.
    /// </summary>
    /// <param name="timeoutSecs">Read timeout in seconds.</param>
    /// <returns>Message payload without delimiter bytes.</returns>
    /// <remarks>
    /// <para><b>Notes:</b> Message framing depends on <see cref="RxDelimiter"/>, which may vary by command family or transport implementation.</para>
    /// </remarks>
    public async Task<string> ReadMessageAsync(double timeoutSecs = DefaultTimeoutSecs)
    {
        return await ReadUntilAsync(RxDelimiter, timeoutSecs).ConfigureAwait(false);
    }

    /// <summary>
    /// Stream helper that reads until a delimiter sequence appears.
    /// </summary>
    /// <param name="stream">Connected stream to read from.</param>
    /// <param name="expected">Expected delimiter sequence.</param>
    /// <param name="timeoutSecs">Read timeout in seconds.</param>
    /// <returns>Decoded Latin-1 payload without delimiter and without XON/XOFF bytes.</returns>
    protected static async Task<string> ReadUntilStreamAsync(Stream stream, byte[] expected, double timeoutSecs)
    {
        var buffer = new List<byte>(256);
        var window = new Queue<byte>(expected.Length);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSecs));

        while (true)
        {
            var one = new byte[1];
            int read;
            try
            {
                read = await stream.ReadAsync(one, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                throw new TimeoutException($"Read timed out after {timeoutSecs:F3}s", ex);
            }

            if (read <= 0)
            {
                throw new DeviceUnavailableException("Transport disconnected while reading");
            }

            var b = one[0];
            buffer.Add(b);
            window.Enqueue(b);
            if (window.Count > expected.Length)
            {
                window.Dequeue();
            }

            if (window.Count == expected.Length && window.SequenceEqual(expected))
            {
                buffer.RemoveRange(buffer.Count - expected.Length, expected.Length);
                return Encoding.Latin1.GetString([.. buffer]).Replace("\x11", string.Empty).Replace("\x13", string.Empty);
            }
        }
    }
}
