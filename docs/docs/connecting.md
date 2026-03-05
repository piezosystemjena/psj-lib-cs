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

``` python
import asyncio
from psj_lib import DDriveDevice, TransportType

async def main():
    # Connect via Serial
    device = DDriveDevice(TransportType.SERIAL, "COM3")  # Windows
    # device = DDriveDevice(TransportType.SERIAL, "/dev/ttyUSB0")  # Linux

    async with device:
        # Device is now connected
        print(f"Connected to {device.device_id}")

asyncio.run(main())
```

## Device Discovery

### Automatic Device Discovery

The `discover_devices()` method automatically finds connected devices:

``` python
import asyncio
from psj_lib import DDriveDevice, DiscoverFlags

async def discover():
    # Discover all connected d-Drive devices
    devices = await DDriveDevice.discover_devices()

    for device in devices:
        print(f"Device: {device.device_id}")
        print(f"Address: {device.address}")
        print(f"Type: {device.transport_type}")
        print("---")

asyncio.run(discover())
```

**Example Output:**

``` text
Device: d-Drive
Address: COM3
Type: TransportType.SERIAL
---
```

### Using Discovery with Connection

Complete discovery and connection example:

``` python
from psj_lib import DDriveDevice

async def discover_and_connect():
    # Discover devices
    devices = await DDriveDevice.discover_devices()

    if not devices:
        print("No devices found")
        return

    # Get first device (already created and ready to connect)
    device = devices[0]

    # Method 1: Connect with async context manager
    async with device:
        print(f"Connected to {device.device_id}")
        channels = device.channels
        print(f"Available channels: {len(channels)}")

    # Method 2: Manual open/close
    await device.connect()
    try:
        print(f"Connected to {device.device_id}")
        # Use device...
    finally:
        await device.disconnect()
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

``` python
from psj_lib import DDriveDevice, TransportType

async def connect_serial():
    # Create device (not connected yet)
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    # Connect using context manager (recommended)
    async with device:
        # Device is now open and connected
        print(f"Connected to {device.device_id}")

        # Access channels
        for channel in device.channels:
            status = await channel.status_register.read()
            print(f"Channel {channel.channel_id}: {status}")

    # Device automatically disconnected when exiting context
```

### Manual Connection Management

If you need manual control over connection lifecycle:

``` python
async def manual_connection():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    try:
        # Manually open connection
        await device.open()
        print("Device opened")

        # Use device...
        channels = device.channels

    finally:
        # Always close when done
        await device.close()
        print("Device closed")
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

### Context Manager Pattern (Recommended)

The context manager automatically handles connection lifecycle:

``` python
async def safe_connection():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    async with device:
        # Connection opens here
        channels = device.channels

        # Do work with device
        for channel in channels:
            await channel.closed_loop_controller.set(True)

    # Connection closes here (even if exception occurs)
```

### Long-Running Connection

For applications that keep device connected:

``` python
class ControlSystem:
    def __init__(self, port: str):
        self.device = DDriveDevice(TransportType.SERIAL, port)

    async def start(self):
        await self.device.open()
        print("System started")

    async def stop(self):
        await self.device.close()
        print("System stopped")

    async def move_channel(self, channel_id: int, position: float):
        channel = self.device.channels[channel_id]
        await channel.setpoint.set(position)

# Usage
system = ControlSystem("COM3")
await system.start()

# Use system...
await system.move_channel(0, 50.0)

# Clean shutdown
await system.stop()
```

### Multiple Devices

Managing multiple devices simultaneously:

``` python
async def multi_device():
    # Create multiple devices
    device1 = DDriveDevice(TransportType.SERIAL, "COM3")
    device2 = DDriveDevice(TransportType.TELNET, "192.168.1.100")

    # Connect both
    async with device1, device2:
        # Both devices connected

        # Parallel operations
        await asyncio.gather(
            device1.channels[0].setpoint.set(30.0),
            device2.channels[0].setpoint.set(60.0)
        )
```

## Error Handling

### Connection Failures

Handle connection errors gracefully:

``` python
from psj_lib import DeviceError, ProtocolError

async def safe_connect():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    try:
        async with device:
            print("Connected successfully")
            # Use device...

    except (DeviceError, ProtocolError) as e:
        print(f"Failed to connect: {e}")
        # Try alternative port or notify user

    except Exception as e:
        print(f"Unexpected error: {e}")
```

### Timeout Configuration

Set timeout for connection attempts:

``` python
async def connect_with_timeout():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    try:
        # Wait up to 5 seconds for connection
        async with asyncio.timeout(5.0):
            await device.connect()
            print("Connected")

    except asyncio.TimeoutError:
        print("Connection timeout - check device power and cables")
```

### Reconnection Logic

Implement automatic reconnection:

``` python
async def connect_with_retry(port: str, max_attempts: int = 3):
    device = DDriveDevice(TransportType.SERIAL, port)

    for attempt in range(max_attempts):
        try:
            await device.open()
            print(f"Connected on attempt {attempt + 1}")
            return device

        except (DeviceError, ProtocolError):
            print(f"Attempt {attempt + 1} failed")
            if attempt < max_attempts - 1:
                await asyncio.sleep(2.0)  # Wait before retry

    raise DeviceConnectionError(f"Failed after {max_attempts} attempts")
```

## Verification

After connecting, verify device is ready:

``` python
async def verify_connection():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    async with device:
        print(f"Device Info: {device.device_info}")

        # Check channels discovered
        print(f"Channels: {len(device.channels)}")

        # Read status from each channel
        for channel in device.channels:
            status = await channel.status_register.get()
            print(f"Channel {channel.id}:")
            print(f"  Closed loop: {status.closed_loop_state}")
            print(f"  Temperature: {await channel.temperature.get():.1f}°C")
```

## Best Practices

1.  **Use Context Managers**: Always use `async with` for automatic
    cleanup
2.  **Check Discovery**: Use discovery before hardcoding ports/addresses
3.  **Handle Errors**: Always catch and handle connection errors
4.  **Verify Connection**: Read device info after connecting
5.  **Clean Shutdown**: Ensure `close()` is called or use context
    manager
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

- Learn basic operations: `getting_started`
- Explore d-Drive features: `d_drive`
- See working examples: `examples`
