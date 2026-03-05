# Connecting to Devices

This guide explains how to discover and connect to piezosystem jena
devices using psj-lib.

## Connection Overview

psj-lib supports two connection methods:

- **Serial (USB)**: Direct USB connection using virtual COM port
- **Telnet (Ethernet)**: Network connection via Telnet protocol

The same high-level API is used regardless of transport type, allowing
seamless switching between connection methods.

Final connectivity support depends on the specific device model.

## Quick Start

The fastest way to connect:

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3"); // Windows
// var device = new DDriveDevice(TransportType.Serial, "/dev/ttyUSB0"); // Linux

await device.ConnectAsync().ConfigureAwait(false);
try
{
    Console.WriteLine($"Connected to {device.DeviceId}");
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Device Discovery

### Automatic Device Discovery

`DiscoverDevicesAsync<TDevice>()` automatically finds connected devices:

``` csharp
using PsjLib.Base;
using PsjLib.DDriveFamily;
using PsjLib.Transport;

// Discover all connected d-Drive amplifiers
var devices = await PiezoDevice.DiscoverDevicesAsync<DDriveDevice>(
    DiscoverFlags.AllInterfaces).ConfigureAwait(false);

foreach (var discovered in devices)
{
    var info = discovered.DeviceInfo;
    Console.WriteLine($"Device: {info.DeviceId}");
    Console.WriteLine($"Identifier: {info.TransportInfo.Identifier}");
    Console.WriteLine($"Type: {info.TransportInfo.Transport}");
    Console.WriteLine("---");
}
```

**Example Output:**

``` text
Device: d-Drive
Address: COM3
Type: Serial
---
```

### Using Discovery with Connection

Complete discovery and connection example:

``` csharp
using PsjLib.Base;
using PsjLib.DDriveFamily;

var devices = await PiezoDevice.DiscoverDevicesAsync<DDriveDevice>().ConfigureAwait(false);
if (devices.Count == 0)
{
    Console.WriteLine("No devices found");
    return;
}

var device = devices[0];
await device.ConnectAsync().ConfigureAwait(false);
try
{
    Console.WriteLine($"Connected to {device.DeviceId}");
    Console.WriteLine($"Available channels: {device.Channels.Count}");
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Serial Connection

### Serial Port Identification

**Windows:**

Ports are named `COM1`, `COM2`, `COM3`, etc.

Check Device Manager → Ports (COM & LPT) to find your device.

**Linux:**

Ports are typically `/dev/ttyUSB0`, `/dev/ttyACM0`, etc.

Use `ls /dev/ttyUSB* /dev/ttyACM*` to list ports.

**macOS:**

Ports appear as `/dev/cu.usbserial-*` or `/dev/tty.usbserial-*`.

Use `ls /dev/cu.* /dev/tty.*` to list ports.

### Creating a Serial Connection

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    Console.WriteLine($"Connected to {device.DeviceId}");
    foreach (var (channelId, channel) in device.Channels)
    {
        var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
        Console.WriteLine($"Channel {channelId}: ClosedLoop={status.ClosedLoop}");
    }
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Manual Connection Management

If you need manual control over connection lifecycle:

``` csharp
var device = new DDriveDevice(TransportType.Serial, "COM3");
try
{
    await device.ConnectAsync().ConfigureAwait(false);
    Console.WriteLine("Device opened");
    var channels = device.Channels;
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
    Console.WriteLine("Device closed");
}
```

### Serial Port Settings

psj-lib automatically configures serial port settings. Most devices use:

- **Baud Rate**: 115200
- **Data Bits**: 8
- **Stop Bits**: 1
- **Parity**: None
- **Flow Control**: None

For NV-family devices, identification is performed at 19200 baud and
transport settings are adjusted by the driver as needed.

## Connection Patterns

### `try/finally` Pattern (Recommended)

The context manager automatically handles connection lifecycle:

``` csharp
var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    foreach (var channel in device.Channels.Values)
    {
        await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    }
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Long-Running Connection

For applications that keep device connected:

``` csharp
public sealed class ControlSystem(string port)
{
    private readonly DDriveDevice _device = new(TransportType.Serial, port);

    public async Task StartAsync() => await _device.ConnectAsync().ConfigureAwait(false);
    public async Task StopAsync() => await _device.CloseAsync().ConfigureAwait(false);

    public async Task MoveChannelAsync(int channelId, double position)
    {
        var channel = _device.Channels[channelId];
        await channel.Setpoint.SetAsync(position).ConfigureAwait(false);
    }
}
```

### Multiple Devices

Managing multiple devices simultaneously:

``` csharp
var device1 = new DDriveDevice(TransportType.Serial, "COM3");
var device2 = new DDriveDevice(TransportType.Telnet, "192.168.1.100");

await Task.WhenAll(device1.ConnectAsync(), device2.ConnectAsync()).ConfigureAwait(false);
try
{
    await Task.WhenAll(
        device1.Channels[0].Setpoint.SetAsync(30.0),
        device2.Channels[0].Setpoint.SetAsync(60.0)
    ).ConfigureAwait(false);
}
finally
{
    await Task.WhenAll(device1.CloseAsync(), device2.CloseAsync()).ConfigureAwait(false);
}
```

## Error Handling

### Connection Failures

Handle connection errors gracefully:

``` csharp
using PsjLib.Base;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
try
{
    await device.ConnectAsync().ConfigureAwait(false);
    Console.WriteLine("Connected successfully");
}
catch (DeviceError ex)
{
    Console.WriteLine($"Device error: {ex.Message}");
}
catch (ProtocolException ex)
{
    Console.WriteLine($"Transport error: {ex.Message}");
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Timeout Configuration

Set timeout for connection attempts:

``` csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var connectTask = device.ConnectAsync();
var completed = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token)).ConfigureAwait(false);
if (completed != connectTask)
{
    Console.WriteLine("Connection timeout - check device power and cables");
}
```

### Reconnection Logic

Implement automatic reconnection:

``` csharp
async Task<DDriveDevice> ConnectWithRetryAsync(string port, int maxAttempts = 3)
{
    var device = new DDriveDevice(TransportType.Serial, port);
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await device.ConnectAsync().ConfigureAwait(false);
            return device;
        }
        catch (Exception) when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }
    }

    throw new DeviceUnavailableException($"Failed after {maxAttempts} attempts");
}
```

## Verification

After connecting, verify device is ready:

``` csharp
await device.ConnectAsync().ConfigureAwait(false);
try
{
    Console.WriteLine($"Device Info: {device.DeviceInfo}");
    Console.WriteLine($"Channels: {device.Channels.Count}");

    foreach (var (channelId, channel) in device.Channels)
    {
        var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
        var temperature = await channel.Temperature.GetAsync().ConfigureAwait(false);
        Console.WriteLine($"Channel {channelId}:");
        Console.WriteLine($"  Closed loop: {status.ClosedLoop}");
        Console.WriteLine($"  Temperature: {temperature:F1}°C");
    }
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Best Practices

1.  **Use `try/finally`**: Always close devices with `CloseAsync()`
2.  **Check Discovery**: Use discovery before hardcoding ports/addresses
3.  **Handle Errors**: Always catch and handle connection errors
4.  **Verify Connection**: Read device info after connecting
5.  **Clean Shutdown**: Ensure `CloseAsync()` is called
6.  **Network Stability**: Use Telnet for permanent installations,
    Serial for development
7.  **Timeout Protection**: Set reasonable timeouts for all operations
8.  **Single Connection**: Don't open multiple connections to same
    device

## Troubleshooting

### "Device Not Found"

**Symptom**: Discovery returns empty list

**Solutions**:

- Check device is powered on
- Verify cable connections
- Check correct transport type (Serial vs Telnet)
- Try manual port/address specification
- Verify drivers installed (Serial)
- Check network connectivity (Telnet)

### "Permission Denied" (Linux Serial)

**Symptom**: Cannot open serial port

**Solution**: Add user to dialout group:

``` bash
sudo usermod -a -G dialout $USER
# Log out and back in
```

### "Connection Timeout"

**Symptom**: Connection attempts timeout

**Solutions**:

- Verify device is not already connected by another application
- Check baud rate (should be auto-detected)
- Try power cycling the device
- For Telnet: verify IP address and network connectivity

### "Port Already in Use"

**Symptom**: Serial port locked by another application

**Solutions**:

- Close any other applications using the device
- Check for zombie processes: `lsof | grep ttyUSB` (Linux)
- Disconnect and reconnect USB cable
- Restart computer as last resort

## Next Steps

Now that you can connect to devices:

- Learn basic operations: [Getting Started](getting_started.md)
- Explore d-Drive features: [d-Drive](d_drive.md)
- See working examples: [Examples](examples.md)
