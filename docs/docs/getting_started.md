# Getting Started

This tutorial guides you through the basics of using PsjLib to control
piezosystem jena devices.

## Your First Program

Let's start with a complete, minimal example:

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    var channel = device.Channels[0];
    var position = await channel.Position.GetAsync().ConfigureAwait(false);
    Console.WriteLine($"Current position: {position:F2} µm");

    await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
    Console.WriteLine("Moved to 50.0 µm");

    var finalPos = await channel.Position.GetAsync().ConfigureAwait(false);
    Console.WriteLine($"Final position: {finalPos:F2} µm");
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

**What This Does:**

1.  Connects to a d-Drive device on COM3
2.  Gets the first channel
3.  Reads the current position
4.  Enables closed-loop control
5.  Moves to 50 µm
6.  Verifies the final position

## Choosing the Right Device Class

Use the class matching your hardware model:

- `DDriveDevice` for d-Drive systems
- `PSJ30DVDevice` for 30DV50/300
- `NV120Device`, `NV120CLEDevice`, `NV403Device`, `NV403CLEDevice` for
  supported NV models

## Understanding the Basics

### Async/Await Pattern

PsjLib uses .NET `Task`-based async/await for non-blocking operations. Key points:

- All device operations are `async` functions
- Use `await` when calling device methods
- Run your async code from an `async Task Main(...)` entry point

``` csharp
// ✓ Correct
var position = await channel.Position.GetAsync().ConfigureAwait(false);

// ✗ Wrong - missing await
var pending = channel.Position.GetAsync();
```

### Device Hierarchy

PsjLib uses a three-level structure:

``` text
Device (DDriveDevice)
└── Channels (DDriveChannel / DDriveFamilyChannel)
    └── Capabilities (Position, StatusRegister, PidController, etc.)
```

``` csharp
// Access pattern
var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
var channel = device.Channels[0];

// Use "Position" capability
var value = await channel.Position.GetAsync().ConfigureAwait(false);
```

## Working with Channels

### Accessing Channels

Devices may have multiple channels:

``` csharp
await device.ConnectAsync().ConfigureAwait(false);
try
{
    Console.WriteLine($"Device has {device.Channels.Count} channels");
    var channel0 = device.Channels[0];

    foreach (var (channelId, _) in device.Channels)
    {
        Console.WriteLine($"Channel {channelId}");
    }
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Channel Information

Each channel provides identification and status:

``` csharp
var channel = device.Channels[0];
Console.WriteLine($"Channel ID: {channel.Id}");
var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Closed-loop enabled: {status.ClosedLoop}");
Console.WriteLine($"No overload: {status.NoOverload}");
Console.WriteLine($"Setpoint reached: {status.SetpointReached}");
```

## Position Control

### Open-Loop Control

Open-loop control sets output voltage directly:

``` csharp
await channel.ClosedLoopController.SetAsync(false).ConfigureAwait(false);
await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
var target = await channel.Setpoint.GetAsync().ConfigureAwait(false); // cached value
```

**Use Cases:**

- Testing piezo response
- Maximum speed movement (no feedback delay)
- Applications where position feedback unavailable

### Closed-Loop Control

Depending on the amplifier and connected actuator, closed-loop control
might be available. It uses sensor feedback for precise positioning:

``` csharp
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
await channel.Setpoint.SetAsync(30.0).ConfigureAwait(false);
var actualPos = await channel.Position.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Position: {actualPos:F2} µm");
```

**Advantages:**

- Precise positioning regardless of load
- Automatic compensation for drift and hysteresis
- Repeatable positioning

### Position Control Example

Complete example with error checking:

``` csharp
static async Task<bool> MoveToPositionAsync(DDriveChannel channel, double target, double tolerance = 0.5)
{
    await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    await channel.Setpoint.SetAsync(target).ConfigureAwait(false);
    await Task.Delay(1000).ConfigureAwait(false);

    var actual = await channel.Position.GetAsync().ConfigureAwait(false);
    var error = Math.Abs(actual - target);
    Console.WriteLine(error < tolerance
        ? $"✓ Reached {target:F2} µm (error: {error:F3} µm)"
        : $"✗ Position error: {error:F3} µm");
    return error < tolerance;
}
```

## Reading Status

### Status Register

Depending on the device, a status register might be available. The
status register provides real-time device state:

``` csharp
var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);

# Check individual flags
Console.WriteLine($"Closed-loop: {status.ClosedLoop}");
Console.WriteLine($"Actor plugged: {status.ActorPlugged}");
Console.WriteLine($"Actor type: {status.ActorType}");
Console.WriteLine($"Sensor type: {status.SensorType}");
```

### Temperature Monitoring

Monitor amplifier temperature:

``` csharp
var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Temperature: {temp:F1}°C");
if (temp > 60.0)
{
    Console.WriteLine("Warning: High temperature!");
}
```

### Actuator Information

Read actuator description string:

``` csharp
var actuator = await channel.ActuatorDescription.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Actuator description: {actuator}");
```

## Basic Control Patterns

### Sequential Movement

Move through a sequence of positions:

``` csharp
var positions = new[] { 10.0, 30.0, 50.0, 70.0, 90.0 };
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
foreach (var pos in positions)
{
    await channel.Setpoint.SetAsync(pos).ConfigureAwait(false);
    await Task.Delay(200).ConfigureAwait(false);
    var actual = await channel.Position.GetAsync().ConfigureAwait(false);
    Console.WriteLine($"Position: {actual:F2} µm");
}
```

### Parallel Channel Control

Control multiple channels simultaneously:

``` csharp
var channels = device.Channels.Values.ToList();
await Task.WhenAll(channels.Select(ch => ch.ClosedLoopController.SetAsync(true))).ConfigureAwait(false);
var targets = new[] { 30.0, 50.0, 70.0 };
await Task.WhenAll(channels.Zip(targets).Select(x => x.First.Setpoint.SetAsync(x.Second))).ConfigureAwait(false);
```

### Continuous Monitoring

Monitor position over time:

``` csharp
var endTime = DateTime.UtcNow + TimeSpan.FromSeconds(5);
while (DateTime.UtcNow < endTime)
{
    var pos = await channel.Position.GetAsync().ConfigureAwait(false);
    var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
    Console.WriteLine($"Position: {pos:F2} µm, Temp: {temp:F1}°C");
    await Task.Delay(100).ConfigureAwait(false);
}
```

## Handling Device Errors

Always handle potential errors:

``` csharp
try
{
    var device = new DDriveDevice(TransportType.Serial, "COM3");
    await device.ConnectAsync().ConfigureAwait(false);
    try
    {
        var channel = device.Channels[0];
        await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
    }
    finally
    {
        await device.CloseAsync().ConfigureAwait(false);
    }
}
catch (DeviceUnavailableException ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}
catch (DeviceError ex)
{
    Console.WriteLine($"Device error: {ex.Message}");
}
```

## Common Patterns

### Initialization Routine

Standard initialization pattern:

``` csharp
var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Initial status: {status}");
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
await channel.Setpoint.SetAsync(0.0).ConfigureAwait(false);
Console.WriteLine("Homed to 0.0 µm");
```

## Complete Example

Here's a complete application template:

``` csharp
using PsjLib.Base;
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var port = "COM3";
var targetPositions = new[] { 20.0, 40.0, 60.0, 80.0 };
var device = new DDriveDevice(TransportType.Serial, port);

try
{
    await device.ConnectAsync().ConfigureAwait(false);
    var channel = device.Channels[0];

    await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
    Console.WriteLine($"Temperature: {temp:F1}°C");

    foreach (var target in targetPositions)
    {
        await channel.Setpoint.SetAsync(target).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);
        var actual = await channel.Position.GetAsync().ConfigureAwait(false);
        var error = Math.Abs(actual - target);
        Console.WriteLine($"Target: {target:F1} µm, Actual: {actual:F2} µm, Error: {error:F3} µm");
    }

    await channel.Setpoint.SetAsync(0.0).ConfigureAwait(false);
}
catch (DeviceError ex)
{
    Console.WriteLine($"Device error: {ex.Message}");
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Best Practices

1.  **Always Use `try/finally`**

    ``` csharp
    await device.ConnectAsync().ConfigureAwait(false);
    try
    {
        // Device operations here
    }
    finally
    {
        await device.CloseAsync().ConfigureAwait(false);
    }
    ```

2.  **Enable Closed-Loop for Precision**

    ``` csharp
    await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    ```

3.  **Check Status After Critical Operations**

    ``` csharp
    await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
    var actual = await channel.Position.GetAsync().ConfigureAwait(false);
    ```

4.  **Handle Errors Appropriately**

    ``` csharp
    try
    {
        await channel.Setpoint.SetAsync(target).ConfigureAwait(false);
    }
    catch (DeviceError)
    {
        // Handle error
    }
    ```

5.  **Monitor Temperature Under Load**

    ``` csharp
    var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
    if (temp > 60.0)
    {
        // Reduce duty cycle or wait
    }
    ```

## Next Steps

Now that you understand the basics:

- Learn about d-Drive specific features: [d-Drive](d_drive.md)
- Explore base capabilities: [Base Capabilities](base_capabilities.md)
- See complete examples: [Examples](examples.md)
