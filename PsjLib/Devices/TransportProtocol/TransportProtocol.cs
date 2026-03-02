using System.Text;

namespace PsjLib.Transport;

public delegate Task<string?> DiscoveryCallback(TransportProtocol transport);

public abstract class TransportProtocol
{
    public static readonly byte[] Xon = [0x11];
    public static readonly byte[] Xoff = [0x13];
    public static readonly byte[] Lf = [0x0A];
    public static readonly byte[] Cr = [0x0D];
    public static readonly byte[] Crlf = [0x0D, 0x0A];

    public const double DefaultTimeoutSecs = 0.6;

    public byte[] RxDelimiter { get; set; } = Xon;

    public abstract TransportType TransportType { get; }
    public abstract bool IsConnected { get; }
    public abstract string Identifier { get; }

    public abstract Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(DiscoveryCallback discoveryCallback);
    public abstract Task ConnectAsync(bool autoAdjustCommParams = true);
    public abstract Task FlushInputAsync();
    public abstract Task WriteAsync(string cmd);
    public abstract Task<string> ReadUntilAsync(byte[] expected, double timeoutSecs = DefaultTimeoutSecs);
    public abstract TransportProtocolInfo GetInfo();
    public abstract Task CloseAsync();

    public async Task<string> ReadMessageAsync(double timeoutSecs = DefaultTimeoutSecs)
    {
        return await ReadUntilAsync(RxDelimiter, timeoutSecs).ConfigureAwait(false);
    }

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
