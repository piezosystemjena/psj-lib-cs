using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace PsjLib.Transport;

/// <summary>
/// Telnet/TCP transport implementation used for Ethernet-connected controllers.
/// </summary>
/// <remarks>
/// <para><b>Notes:</b> Endpoints can be addressed either directly by IP address or indirectly via Lantronix MAC address discovery.</para>
/// <para><b>Notes:</b> Network discovery depends on UDP broadcast support and firewall/network policy.</para>
/// </remarks>
internal sealed class TelnetProtocol : TransportProtocol
{
    private const string BroadcastIp = "255.255.255.255";
    private const int DiscoveryUdpPort = 30718;
    private const int ConfigPort = 9999;
    private const string LantronixMacPrefix = "00:80:A3";

    private string _host;
    private readonly int _port;
    private readonly string? _mac;
    private TcpClient? _client;
    private NetworkStream? _stream;

    private enum FlowControlMode
    {
        NoFlowControl = 0x00,
        XonXoff = 0x01,
        RtsCts = 0x02,
        XonXoffPassToHost = 0x05,
    }

    private sealed record NetworkEndpoint(string Mac, string Ip);

    /// <summary>
    /// Initializes a telnet transport endpoint.
    /// </summary>
    /// <param name="identifier">
    /// Target host identifier. This can be an IP address or a Lantronix MAC address.
    /// </param>
    /// <param name="port">TCP port used for device communication.</param>
    internal TelnetProtocol(string identifier = "", int port = 23)
    {
        _port = port;
        if (IPAddress.TryParse(identifier, out var _))
        {
            _host = identifier;
        }
        else
        {
            _host = string.Empty;
            _mac = NormalizeMac(identifier);
        }
    }

    /// <inheritdoc/>
    internal override TransportType TransportType => TransportType.Telnet;
    /// <inheritdoc/>
    internal override bool IsConnected => _client?.Connected == true;
    /// <inheritdoc/>
    internal override string Identifier => string.IsNullOrWhiteSpace(_host) ? (_mac ?? string.Empty) : _host;

    /// <inheritdoc/>
    internal override async Task<IReadOnlyList<DetectedDevice>> DiscoverDevicesAsync(DiscoveryCallback discoveryCallback)
    {
        var devices = new List<DetectedDevice>();

        var endpoints = await DiscoverLantronixDevicesAsync().ConfigureAwait(false);
        foreach (var endpoint in endpoints)
        {
            var protocol = new TelnetProtocol(endpoint.Ip, _port);
            try
            {
                await protocol.ConnectAsync(autoAdjustCommParams: false).ConfigureAwait(false);
                var deviceId = await discoveryCallback(protocol).ConfigureAwait(false);
                if (deviceId is not null)
                {
                    devices.Add(new DetectedDevice(TransportType.Telnet, endpoint.Ip, endpoint.Mac, deviceId));
                }
            }
            catch
            {
            }
            finally
            {
                await protocol.CloseAsync().ConfigureAwait(false);
            }
        }

        return devices;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Notes:</b> When MAC addressing is used, connection performs an additional discovery phase to resolve the current IP.</para>
    /// <para><b>Notes:</b> Optional communication-parameter adjustment can trigger a device-side reboot delay before regular Telnet communication resumes.</para>
    /// </remarks>
    internal override async Task ConnectAsync(bool autoAdjustCommParams = true)
    {
        if (IsConnected)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_host))
        {
            if (string.IsNullOrWhiteSpace(_mac))
            {
                throw new DeviceUnavailableException("Either host IP or Lantronix MAC address is required");
            }

            _host = await DiscoverLantronixDeviceAsync(_mac).ConfigureAwait(false)
                ?? throw new DeviceUnavailableException($"Device with MAC {_mac} not found");
        }

        if (autoAdjustCommParams)
        {
            _ = await ConfigureFlowControlModeAsync(_host, FlowControlMode.XonXoffPassToHost).ConfigureAwait(false);
        }

        _client = new TcpClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            await _client.ConnectAsync(_host, _port, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DeviceUnavailableException($"Cannot connect to telnet host {_host}:{_port}", ex);
        }

        _stream = _client.GetStream();
    }

    /// <inheritdoc/>
    internal override async Task FlushInputAsync()
    {
        if (_stream is null)
        {
            return;
        }

        var buffer = new byte[1024];
        while (_stream.DataAvailable)
        {
            await _stream.ReadAsync(buffer).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    internal override async Task WriteAsync(string cmd)
    {
        if (_stream is null)
        {
            throw new DeviceUnavailableException("Telnet device not connected");
        }

        await FlushInputAsync().ConfigureAwait(false);
        var bytes = Encoding.Latin1.GetBytes(cmd);
        await _stream.WriteAsync(bytes).ConfigureAwait(false);
        await _stream.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    internal override async Task<string> ReadUntilAsync(byte[] expected, double timeoutSecs = DefaultTimeoutSecs)
    {
        if (_stream is null)
        {
            throw new DeviceUnavailableException("Telnet device not connected");
        }

        return await ReadUntilStreamAsync(_stream, expected, timeoutSecs).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    internal override TransportProtocolInfo GetInfo() => new(TransportType.Telnet, _host, _mac);

    /// <inheritdoc/>
    internal override Task CloseAsync()
    {
        _stream?.Dispose();
        _stream = null;
        _client?.Dispose();
        _client = null;
        return Task.CompletedTask;
    }

    private static string? NormalizeMac(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac))
        {
            return null;
        }

        var stripped = Regex.Replace(mac.Trim(), "[^0-9A-Fa-f]", string.Empty);
        if (stripped.Length != 12)
        {
            return null;
        }

        return string.Join(':', Enumerable.Range(0, 6).Select(i => stripped.Substring(i * 2, 2).ToUpperInvariant()));
    }

    private static IEnumerable<IPAddress> GetActiveEthernetIpv4Addresses()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(static ni => ni.OperationalStatus == OperationalStatus.Up)
            .Where(static ni => ni.NetworkInterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211)
            .SelectMany(static ni => ni.GetIPProperties().UnicastAddresses)
            .Select(static ua => ua.Address)
            .Where(static ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
    }

    private static async Task<List<(byte[] Data, IPEndPoint Remote)>> SendUdpBroadcastAsync(IPAddress localIp)
    {
        using var udp = new UdpClient(new IPEndPoint(localIp, DiscoveryUdpPort));
        udp.EnableBroadcast = true;
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        var discoveryPacket = new byte[] { 0x00, 0x00, 0x00, 0xF6 };
        await udp.SendAsync(discoveryPacket, discoveryPacket.Length, new IPEndPoint(IPAddress.Parse(BroadcastIp), DiscoveryUdpPort)).ConfigureAwait(false);

        var responses = new List<(byte[] Data, IPEndPoint Remote)>();
        var deadline = DateTime.UtcNow.AddMilliseconds(400);

        while (DateTime.UtcNow < deadline)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            using var timeoutCts = new CancellationTokenSource(remaining);
            try
            {
                var result = await udp.ReceiveAsync(timeoutCts.Token).ConfigureAwait(false);
                responses.Add((result.Buffer, result.RemoteEndPoint));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }
        }

        return responses;
    }

    private static IReadOnlyList<NetworkEndpoint> ParseDiscoveryResponses(IEnumerable<(byte[] Data, IPEndPoint Remote)> responses)
    {
        var parsed = new List<NetworkEndpoint>();

        foreach (var (data, remote) in responses)
        {
            if (data.Length != 30)
            {
                continue;
            }

            var macBytes = data.Skip(24).Take(6).ToArray();
            var mac = string.Join(':', macBytes.Select(static b => b.ToString("X2")));
            if (!mac.StartsWith(LantronixMacPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            parsed.Add(new NetworkEndpoint(mac.ToUpperInvariant(), remote.Address.ToString()));
        }

        return parsed;
    }

    private static async Task<IReadOnlyList<NetworkEndpoint>> DiscoverLantronixDevicesAsync()
    {
        var tasks = GetActiveEthernetIpv4Addresses().Select(SendUdpBroadcastAsync).ToList();
        if (tasks.Count == 0)
        {
            return [];
        }

        var allResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        var endpoints = allResults.SelectMany(ParseDiscoveryResponses);

        return endpoints
            .GroupBy(static e => $"{e.Mac}-{e.Ip}")
            .Select(static g => g.First())
            .ToList();
    }

    private static async Task<string?> DiscoverLantronixDeviceAsync(string targetMac)
    {
        var normalized = NormalizeMac(targetMac);
        if (normalized is null)
        {
            return null;
        }

        var devices = await DiscoverLantronixDevicesAsync().ConfigureAwait(false);
        return devices.FirstOrDefault(d => d.Mac.Equals(normalized, StringComparison.OrdinalIgnoreCase))?.Ip;
    }

    private static async Task<string> ReadAvailableAsync(NetworkStream stream, int timeoutMs, int maxBytes = 4096)
    {
        var buffer = new byte[maxBytes];
        using var cts = new CancellationTokenSource(timeoutMs);
        var builder = new StringBuilder();

        while (!cts.IsCancellationRequested)
        {
            if (!stream.DataAvailable)
            {
                try
                {
                    await Task.Delay(10, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                continue;
            }

            int read;
            try
            {
                read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (read <= 0)
            {
                break;
            }

            builder.Append(Encoding.Latin1.GetString(buffer, 0, read));
            try
            {
                await Task.Delay(20, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            if (!stream.DataAvailable)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static async Task WriteStringAsync(NetworkStream stream, string text)
    {
        var bytes = Encoding.Latin1.GetBytes(text);
        await stream.WriteAsync(bytes).ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    private static async Task VerifyRebootedAsync(string host, int timeoutSecs)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSecs);

        while (DateTime.UtcNow < deadline)
        {
            using var probe = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try
            {
                await probe.ConnectAsync(host, ConfigPort, cts.Token).ConfigureAwait(false);
                return;
            }
            catch
            {
                await Task.Delay(300).ConfigureAwait(false);
            }
        }

        throw new TimeoutException($"Timeout: Unable to connect to {host}:{ConfigPort} within {timeoutSecs} seconds");
    }

    private static async Task<bool> ConfigureFlowControlModeAsync(string host, FlowControlMode mode)
    {
        const int channel1ParamCount = 19;
        const int paramFlowControl = 2;

        using var configClient = new TcpClient();
        using (var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(7)))
        {
            await configClient.ConnectAsync(host, ConfigPort, connectCts.Token).ConfigureAwait(false);
        }

        await using var configStream = configClient.GetStream();

        _ = await ReadAvailableAsync(configStream, 300).ConfigureAwait(false);
        await WriteStringAsync(configStream, "\r").ConfigureAwait(false);
        await Task.Delay(100).ConfigureAwait(false);
        var configuration = await ReadAvailableAsync(configStream, 1200).ConfigureAwait(false);

        if (configuration.Contains($"Flow 0{(int)mode}", StringComparison.Ordinal))
        {
            return false;
        }

        await WriteStringAsync(configStream, "1\r").ConfigureAwait(false);
        for (var index = 0; index < channel1ParamCount; index++)
        {
            await Task.Delay(50).ConfigureAwait(false);
            _ = await ReadAvailableAsync(configStream, 250).ConfigureAwait(false);
            var valueToWrite = index == paramFlowControl ? $"0{(int)mode}\r" : "\r";
            await WriteStringAsync(configStream, valueToWrite).ConfigureAwait(false);
        }

        await Task.Delay(50).ConfigureAwait(false);
        _ = await ReadAvailableAsync(configStream, 250).ConfigureAwait(false);
        await WriteStringAsync(configStream, "9\r").ConfigureAwait(false);
        _ = await ReadAvailableAsync(configStream, 300).ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);

        configStream.Close();
        configClient.Close();

        await VerifyRebootedAsync(host, 25).ConfigureAwait(false);
        return true;
    }
}
