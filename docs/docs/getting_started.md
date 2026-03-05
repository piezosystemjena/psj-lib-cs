# Getting Started

This tutorial guides you through the basics of using psj-lib to control
piezosystem jena devices.

## Your First Program

Let's start with a complete, minimal example:

``` python
import asyncio
from psj_lib import DDriveDevice, TransportType

async def main():
    # Connect to device
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    async with device:
        # Get first channel
        channel = device.channels[0]

        # Read current position
        position = await channel.position.get()
        print(f"Current position: {position:.2f} µm")

        # Enable closed-loop control
        await channel.closed_loop_controller.set(True)

        # Move to target position
        await channel.setpoint.set(50.0)
        print("Moved to 50.0 µm")

        # Read final position
        final_pos = await channel.position.get()
        print(f"Final position: {final_pos:.2f} µm")

if __name__ == "__main__":
    asyncio.run(main())
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

psj-lib uses Python's `asyncio` for non-blocking operations. Key points:

- All device operations are `async` functions
- Use `await` when calling device methods
- Run your async code with `asyncio.run()`

``` python
# ✓ Correct
position = await channel.position.get()

# ✗ Wrong - missing await
position = channel.position.get()  # Returns coroutine, not value
```

### Device Hierarchy

psj-lib uses a three-level structure:

``` text
Device (DDriveDevice)
└── Channels (DDriveChannel)
    └── Capabilities (Position, Status, PID, etc.)
```

``` python
# Access pattern
device = DDriveDevice(TransportType.SERIAL, "COM3")
channel = device.channels[0]  # First channel
position_capability = channel.position

# Use capability
value = await position_capability.get()
```

## Working with Channels

### Accessing Channels

Devices may have multiple channels:

``` python
async with device:
    # Get all channels
    channels = device.channels
    print(f"Device has {len(channels)} channels")

    # Access by index
    channel0 = device.channels[0]
    channel1 = device.channels[1]

    # Iterate over channels
    for channel in device.channels:
        print(f"Channel {channel.channel_id}")
```

### Channel Information

Each channel provides identification and status:

``` python
channel = device.channels[0]

# Channel ID
print(f"Channel ID: {channel.channel_id}")

# Read status register
status = await channel.status_register.read()
print(f"Closed-loop enabled: {status.closed_loop_state}")
print(f"No overload: {status.no_overload}")
print(f"Setpoint reached: {status.setpoint_reached}")
```

## Position Control

### Open-Loop Control

Open-loop control sets output voltage directly:

``` python
# Disable closed-loop for open-loop control
await channel.closed_loop_controller.set(False)

# Set output voltage (0-100V for typical piezo)
await channel.setpoint.set(50.0)  # 50V

# Note: For d-Drive, you can read back via channel.setpoint.get() (returns cached value)
```

**Use Cases:**

- Testing piezo response
- Maximum speed movement (no feedback delay)
- Applications where position feedback unavailable

### Closed-Loop Control

Depending on the amplifier and connected actuator, closed-loop control
might be available. It uses sensor feedback for precise positioning:

``` python
# Enable closed-loop control
await channel.closed_loop_controller.set(True)

# Move to target position (in µm)
await channel.setpoint.set(30.0)

# Read actual position
actual_pos = await channel.position.get()
print(f"Position: {actual_pos:.2f} µm")
```

**Advantages:**

- Precise positioning regardless of load
- Automatic compensation for drift and hysteresis
- Repeatable positioning

### Position Control Example

Complete example with error checking:

``` python
async def move_to_position(channel, target: float, tolerance: float = 0.5):
    """Move to target position and verify arrival."""
    # Enable closed-loop
    await channel.closed_loop_controller.set(True)

    # Set target
    await channel.setpoint.set(target)

    # Wait briefly for settling
    await asyncio.sleep(1)

    # Verify position
    actual = await channel.position.get()
    error = abs(actual - target)

    if error < tolerance:
        print(f"✓ Reached {target:.2f} µm (error: {error:.3f} µm)")
        return True
    else:
        print(f"✗ Position error: {error:.3f} µm")
        return False

# Use it
success = await move_to_position(channel, 50.0)
```

## Reading Status

### Status Register

Depending on the device, a status register might be available. The
status register provides real-time device state:

``` python
status = await channel.status_register.get()

# Check individual flags
print(f"Closed-loop: {status.closed_loop}")
print(f"Actor plugged: {status.actor_plugged}")
print(f"Actor type: {status.actor_type}")
print(f"Sensor type: {status.sensor_type}")
```

### Temperature Monitoring

Monitor amplifier temperature:

``` python
# Read temperature
temp = await channel.temperature.get()
print(f"Temperature: {temp:.1f}°C")

# Check if overheating
if temp > 60.0:
    print("Warning: High temperature!")
    # Take action (reduce duty cycle, enable cooling, etc.)
```

### Actuator Information

Read actuator description string:

``` python
# Get actuator description
actuator = await channel.actuator_description.get()

print(f"Actuator description: {actuator}")
```

## Basic Control Patterns

### Sequential Movement

Move through a sequence of positions:

``` python
async def sequential_scan():
    channel = device.channels[0]
    await channel.closed_loop_controller.set(True)

    positions = [10.0, 30.0, 50.0, 70.0, 90.0]

    for pos in positions:
        await channel.setpoint.set(pos)
        await asyncio.sleep(0.2)  # Dwell time

        actual = await channel.position.get()
        print(f"Position: {actual:.2f} µm")
```

### Parallel Channel Control

Control multiple channels simultaneously:

``` python
async def move_all_channels():
    channels = device.channels

    # Enable closed-loop on all channels
    await asyncio.gather(*[
        ch.closed_loop_controller.set(True)
        for ch in channels
    ])

    # Move all channels to different positions
    targets = [30.0, 50.0, 70.0]
    await asyncio.gather(*[
        ch.setpoint.set(target)
        for ch, target in zip(channels, targets)
    ])
```

### Continuous Monitoring

Monitor position over time:

``` python
async def monitor_position(duration: float = 5.0, interval: float = 0.1):
    """Monitor position for specified duration."""
    channel = device.channels[0]

    end_time = asyncio.get_event_loop().time() + duration

    while asyncio.get_event_loop().time() < end_time:
        pos = await channel.position.get()
        temp = await channel.temperature.get()

        print(f"Position: {pos:.2f} µm, Temp: {temp:.1f}°C")

        await asyncio.sleep(interval)
```

## Error Handling

### Handling Device Errors

Always handle potential errors:

``` python
from psj_lib import DeviceError, DeviceUnavailableException

async def safe_operation():
    try:
        device = DDriveDevice(TransportType.SERIAL, "COM3")

        async with device:
            channel = device.channels[0]
            await channel.setpoint.set(50.0)

    except DeviceUnavailableException as e:
        print(f"Connection failed: {e}")

    except DeviceError as e:
        print(f"Device error: {e}")

    except Exception as e:
        print(f"Unexpected error: {e}")
```

### Timeout Protection

Protect against hanging operations:

``` python
async def safe_move_with_timeout(channel, target: float, timeout: float = 5.0):
    """Move with timeout protection."""
    try:
        async with asyncio.timeout(timeout):
            await channel.setpoint.set(target)
            print(f"Moved to {target:.2f} µm")

    except asyncio.TimeoutError:
        print(f"Move timeout after {timeout}s")
        # Could implement recovery here
```

## Common Patterns

### Initialization Routine

Standard initialization pattern:

``` python
async def initialize_channel(channel):
    """Initialize channel for operation."""
    # Read and display current state
    status = await channel.status_register.get()
    print(f"Initial status: {status}")

    # Enable closed-loop
    await channel.closed_loop_controller.set(True)
    print("Closed-loop enabled")

    # Move to zero position
    await channel.setpoint.set(0.0)
    print("Homed to 0.0 µm")

    return True
```

## Complete Example

Here's a complete application template:

``` python
import asyncio
from psj_lib import DDriveDevice, TransportType
from psj_lib import DeviceError

async def main():
    # Configuration
    port = "COM3"
    target_positions = [20.0, 40.0, 60.0, 80.0]

    try:
        # Connect
        device = DDriveDevice(TransportType.SERIAL, port)
        print(f"Connecting to {port}...")

        async with device:
            print(f"Connected to {device.device_id}")

            # Get channel
            channel = device.channels[0]

            # Initialize
            await channel.closed_loop_controller.set(True)
            temp = await channel.temperature.get()
            print(f"Temperature: {temp:.1f}°C")

            # Execute movement sequence
            for target in target_positions:
                await channel.setpoint.set(target)
                await asyncio.sleep(1)  # Settling time

                actual = await channel.position.get()
                error = abs(actual - target)
                print(f"Target: {target:.1f} µm, Actual: {actual:.2f} µm, "
                      f"Error: {error:.3f} µm")

            # Return to zero
            await channel.setpoint.set(0.0)
            print("Returned to zero")

    except DeviceError as e:
        print(f"Device error: {e}")
        return 1

    except Exception as e:
        print(f"Error: {e}")
        return 1

    print("Completed successfully")
    return 0

if __name__ == "__main__":
    exit_code = asyncio.run(main())
    exit(exit_code)
```

## Best Practices

1.  **Always Use Context Managers**

    ``` python
    async with device:
        # Device operations here
    # Automatically closes
    ```

2.  **Enable Closed-Loop for Precision**

    ``` python
    await channel.closed_loop_controller.set(True)
    ```

3.  **Check Status After Critical Operations**

    ``` python
    await channel.setpoint.set(50.0)
    actual = await channel.position.get()
    ```

4.  **Handle Errors Appropriately**

    ``` python
    try:
        await channel.setpoint.set(target)
    except DeviceError as e:
        # Handle error
    ```

5.  **Monitor Temperature Under Load**

    ``` python
    temp = await channel.temperature.get()
    if temp > 60.0:
        # Reduce duty cycle or wait
    ```

## Next Steps

Now that you understand the basics:

- Learn about d-Drive specific features: `d_drive`
- Explore base capabilities: `base_capabilities`
- See complete examples: `examples`
