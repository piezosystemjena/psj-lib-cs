using System.IO.Ports;
using System.Text;

namespace PsjLib.Transport;

/// <summary>
/// Serial/USB transport implementation for piezo devices.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Discovery and communication require OS-level access to serial ports and may fail when ports are already occupied by other applications.</para>
/// </remarks>
internal sealed class SerialProtocol : TransportProtocol
{
    private readonly string _port;
    private int _baudrate;
    private SerialPort? _serial;

    /// <summary>
    /// Initializes a serial transport endpoint.
    /// </summary>
    /// <param name="identifier">Serial port name (for example <c>COM3</c>).</param>
    /// <param name="baudrate">Serial baud rate used for communication.</param>
    internal SerialProtocol(string identifier, int baudrate = 115200)
    {
        _port = identifier;
        _baudrate = baudrate;
    }

    /// <inheritdoc/>
    internal override TransportType TransportType => TransportType.Serial;
    /// <inheritdoc/>
    internal override bool IsConnected => _serial?.IsOpen == true;
    /// <inheritdoc/>
    internal override string Identifier => _port;

    /// <inheritdoc/>
    internal override async Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(DiscoveryCallback discoveryCallback)
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

    /// <inheritdoc/>
    internal override Task ConnectAsync(bool autoAdjustCommParams = true)
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
                ReadTimeout = 100,
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

    /// <inheritdoc/>
    internal override Task FlushInputAsync()
    {
        _serial?.DiscardInBuffer();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    internal override async Task WriteAsync(string cmd)
    {
        if (_serial is null || !_serial.IsOpen)
        {
            throw new DeviceUnavailableException("Serial device not connected");
        }

        await FlushInputAsync().ConfigureAwait(false);
        await Task.Run(() => _serial.Write(cmd)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    internal override async Task<string> ReadUntilAsync(byte[] expected, double timeoutSecs = DefaultTimeoutSecs)
    {
        if (_serial is null || !_serial.IsOpen)
        {
            throw new DeviceUnavailableException("Serial device not connected");
        }

        if (timeoutSecs <= 0)
        {
            throw new TimeoutException($"Serial read timeout after {timeoutSecs:F3}s");
        }

        var buffer = new List<byte>(256);
        var window = new Queue<byte>(expected.Length);
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSecs);

        while (true)
        {
            int raw;
            try
            {
                raw = await Task.Run(_serial.ReadByte).ConfigureAwait(false);
            }
            catch (System.TimeoutException ex)
            {
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"Serial read timeout after {timeoutSecs:F3}s", ex);
                }

                continue;
            }

            if (raw < 0)
            {
                throw new DeviceUnavailableException("Serial disconnected while reading");
            }

            var one = (byte)raw;

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

    /// <inheritdoc/>
    internal override TransportProtocolInfo GetInfo() => new(TransportType.Serial, _port);

    /// <inheritdoc/>
    internal override void SetProperty(string name, object value)
    {
        if (name.Equals("baudrate", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = value switch
            {
                int baudrate => baudrate,
                _ => Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture),
            };

            _baudrate = parsed;

            if (_serial is not null)
            {
                _serial.BaudRate = _baudrate;
            }
        }
    }

    /// <inheritdoc/>
    internal override object? GetProperty(string name)
    {
        if (name.Equals("baudrate", StringComparison.OrdinalIgnoreCase))
        {
            return _baudrate;
        }

        return null;
    }

    /// <inheritdoc/>
    internal override Task CloseAsync()
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
