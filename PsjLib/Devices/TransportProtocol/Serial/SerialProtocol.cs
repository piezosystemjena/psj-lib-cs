using System.IO.Ports;
using System.Text;

namespace PsjLib.Transport;

public sealed class SerialProtocol : TransportProtocol
{
    private readonly string _port;
    private readonly int _baudrate;
    private SerialPort? _serial;

    public SerialProtocol(string identifier, int baudrate = 115200)
    {
        _port = identifier;
        _baudrate = baudrate;
    }

    public override TransportType TransportType => TransportType.Serial;
    public override bool IsConnected => _serial?.IsOpen == true;
    public override string Identifier => _port;

    public override async Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(DiscoveryCallback discoveryCallback)
    {
        var ports = SerialPort.GetPortNames();
        var tasks = ports.Select(async port =>
        {
            var protocol = new SerialProtocol(port, _baudrate);
            try
            {
                await protocol.ConnectAsync().ConfigureAwait(false);
                var deviceId = await discoveryCallback(protocol).ConfigureAwait(false);
                return deviceId is null ? null : new DetectedDevice(TransportType.Serial, port, DeviceId: deviceId);
            }
            catch
            {
                return null;
            }
            finally
            {
                await protocol.CloseAsync().ConfigureAwait(false);
            }
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Where(static x => x is not null).Cast<DetectedDevice>().ToList();
    }

    public override Task ConnectAsync(bool autoAdjustCommParams = true)
    {
        if (IsConnected)
        {
            return Task.CompletedTask;
        }

        try
        {
            _serial = new SerialPort(_port, _baudrate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = Encoding.Latin1,
                ReadTimeout = 200,
                WriteTimeout = 200,
            };
            _serial.Open();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new DeviceUnavailableException($"Cannot open serial port {_port}", ex);
        }
    }

    public override Task FlushInputAsync()
    {
        _serial?.DiscardInBuffer();
        return Task.CompletedTask;
    }

    public override async Task WriteAsync(string cmd)
    {
        if (_serial is null || !_serial.IsOpen)
        {
            throw new DeviceUnavailableException("Serial device not connected");
        }

        await FlushInputAsync().ConfigureAwait(false);
        await Task.Run(() => _serial.Write(cmd)).ConfigureAwait(false);
    }

    public override async Task<string> ReadUntilAsync(byte[] expected, double timeoutSecs = DefaultTimeoutSecs)
    {
        if (_serial is null || !_serial.IsOpen)
        {
            throw new DeviceUnavailableException("Serial device not connected");
        }

        var buffer = new List<byte>(256);
        var window = new Queue<byte>(expected.Length);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSecs));

        while (true)
        {
            byte one;
            try
            {
                one = await Task.Run(_serial.ReadByte, cts.Token).ConfigureAwait(false) switch
                {
                    < 0 => throw new DeviceUnavailableException("Serial disconnected while reading"),
                    var x => (byte)x,
                };
            }
            catch (OperationCanceledException ex)
            {
                throw new TimeoutException($"Serial read timeout after {timeoutSecs:F3}s", ex);
            }

            buffer.Add(one);
            window.Enqueue(one);
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

    public override TransportProtocolInfo GetInfo() => new(TransportType.Serial, _port);

    public override Task CloseAsync()
    {
        if (_serial?.IsOpen == true)
        {
            _serial.Close();
        }

        _serial?.Dispose();
        _serial = null;
        return Task.CompletedTask;
    }
}
