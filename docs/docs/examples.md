# Examples

This page demonstrates common use cases with complete, working examples.

## Example Scripts

All examples are available in the `examples/` directory of the
repository.

### 01 - Device Discovery and Connection

Demonstrates device discovery, connection, and basic information
retrieval.

**What You'll Learn:**

- How to discover devices on Serial and Telnet
- Connecting to a device
- Displaying device information
- Listing available channels

**Code:** `examples/01_device_discovery_and_connection.py`

``` python
from psj_lib import PiezoDevice, DDriveDevice, DiscoverFlags

# Discover devices on all interfaces
devices = await DDriveDevice.discover_devices(
    flags=DiscoverFlags.ALL_INTERFACES
)

# Display discovered devices
for i, device in enumerate(devices):
    print(f"{i}: {device.device_id} on {device.address}")

# Connect to first device
if devices:
    device = devices[0]  # Already a DDriveDevice instance
    await device.connect()

    # Display channels
    for channel in device.channels:
        print(f"Channel {channel.channel_id}")
```

### 02 - Simple Position Control

Basic position control with open-loop and closed-loop modes.

**What You'll Learn:**

- Reading current position
- Setting target positions
- Open-loop vs closed-loop control
- Position error checking

**Code:** `examples/02_simple_position_control.py`

``` python
# Enable closed-loop control
await channel.closed_loop_controller.set(True)

# Move to target positions
targets = [30.0, 50.0, 70.0]

for target in targets:
    await channel.position.set(target)
    await asyncio.sleep(0.2)  # Settling time

    actual = await channel.position.get()
    error = abs(actual - target)
    print(f"Target: {target:.1f} µm, Actual: {actual:.2f} µm, "
          f"Error: {error:.3f} µm")
```

### 03 - PID Tuning

Configure and test PID controller parameters.

**What You'll Learn:**

- Setting P, I, D parameters
- Reading current PID configuration
- Testing step response

**Code:** `examples/03_pid_tuning.py`

``` python
# Set PID parameters
await channel.pid_controller.set_p(0.5)
await channel.pid_controller.set_i(0.1)
await channel.pid_controller.set_d(0.05)

# Read back settings
p = await channel.pid_controller.get_p()
i = await channel.pid_controller.get_i()
d = await channel.pid_controller.get_d()

print(f"PID Parameters: P={p:.3f}, I={i:.3f}, D={d:.3f}")
```

### 04 - Data Recorder Capture

Record position and voltage data at high sample rates.

**What You'll Learn:**

- Configuring sample rate and buffer
- Starting and stopping recording
- Retrieving and saving data
- Optional plotting with matplotlib

**Code:** `examples/04_data_recorder_capture.py`

``` python
# Configure recorder
await channel.data_recorder.set_sample_rate(10000)  # 10 kHz
await channel.data_recorder.set_num_samples(5000)   # 0.5 seconds

# Start recording
await channel.data_recorder.start()

# Perform movement
await channel.position.set(50.0)

# Wait for recording
await asyncio.sleep(0.6)

# Get data
data = await channel.data_recorder.get_data()

# Export to CSV
with open('recording.csv', 'w') as f:
    f.write('Time,Position,Voltage\n')
    for i in range(len(data['position'])):
        t = i / 10000
        f.write(f"{t:.6f},{data['position'][i]:.4f},"
                f"{data['voltage'][i]:.4f}\n")
```

### 05 - Waveform Generation Basics

Generate periodic waveforms for scanning and testing.

**What You'll Learn:**

- Configuring different waveform types
- Setting frequency and amplitude
- Starting and stopping waveforms

**Code:** `examples/05_waveform_generation_basics.py`

``` python
# Sine wave
await channel.waveform_generator.sine.set_frequency(100.0)
await channel.waveform_generator.sine.set_amplitude(20.0)
await channel.waveform_generator.sine.set_offset(50.0)
await channel.waveform_generator.enable()

# Triangle wave (for scanning)
await channel.waveform_generator.triangle.set_frequency(10.0)
await channel.waveform_generator.triangle.set_amplitude(30.0)
await channel.waveform_generator.enable()

# Sweep (frequency response)
await channel.waveform_generator.sweep.set_start_frequency(10)
await channel.waveform_generator.sweep.set_stop_frequency(5000)
await channel.waveform_generator.sweep.set_duration(10.0)
await channel.waveform_generator.enable()
```

### 06 - Filter Configuration

Configure notch filter, LPF, and error LPF for optimal performance.

**What You'll Learn:**

- Enabling and configuring notch filter
- Setting low-pass filter cutoff
- Error low-pass filter setup

**Code:** `examples/06_filter_configuration.py`

``` python
# Notch filter (suppress resonance)
await channel.notch.set(frequency=2500.0, bandwidth=250.0, enabled=True)

# Low-pass filter (reduce noise)
await channel.lpf.set(cutoff_frequency=1000.0, enabled=True)

# Error low-pass filter (PID stability)
await channel.error_lpf.set(cutoff_frequency=200.0, enabled=True)
```

### 07 - Backup and Restore Configuration

Save and restore device configuration to/from files.

**What You'll Learn:**

- Backing up channel configuration
- Saving to JSON file
- Restoring configuration from file

**Code:** `examples/07_backup_and_restore_configuration.py`

``` python
import json

# Backup configuration
config = {}

# PID parameters
config['pid'] = {
    'p': await channel.pid_controller.get_p(),
    'i': await channel.pid_controller.get_i(),
    'd': await channel.pid_controller.get_d()
}

# Filter settings
config['notch_freq'] = await channel.notch.get_frequency()
config['lpf_cutoff'] = await channel.lpf.get_cutoff_frequency()

# Save to file
with open('device_config.json', 'w') as f:
    json.dump(config, f, indent=2)

# Restore from file
with open('device_config.json', 'r') as f:
    config = json.load(f)

await channel.pid_controller.set_p(config['pid']['p'])
await channel.pid_controller.set_i(config['pid']['i'])
await channel.pid_controller.set_d(config['pid']['d'])
```

### 08 - NV403CLE Capabilities Overview

Demonstrates key NV403CLE capabilities in one workflow.

**What You'll Learn:**

- Backing up configuration at start and restoring it at end
- Setting modulation source to `SERIAL` at startup
- Reading and writing display brightness
- Toggling closed-loop control
- Reading open-loop/closed-loop units and limits
- Reading `actuator connected` status for all channels
- Using `multi_setpoint` and `multi_position`

**Code:** `examples/08_nv403cle_capabilities_overview.py`

``` python
from psj_lib import NV403CLEDevice, NVModulationSourceTypes, TransportType

device = NV403CLEDevice(TransportType.SERIAL, "COM1")
await device.connect()
backup_data = await device.backup()

try:
    # Set required modulation source at startup for all channels
    for ch in device.channels.values():
        await ch.modulation_source.set_source(NVModulationSourceTypes.SERIAL)

    channel = device.channels[0]

    # Display brightness
    await device.display.set(brightness=40.0)

    # Closed-loop toggle
    enabled = await channel.closed_loop_controller.get_enabled()
    await channel.closed_loop_controller.set(not enabled)

    # Unit and limit readout
    open_unit = await channel.openloop_unit.get()
    open_limits = await channel.openloop_limits.get_range()
    closed_unit = await channel.closedloop_unit.get()
    closed_limits = await channel.closedloop_limits.get_range()

    # Status readout for all channels
    for channel_id, ch in device.channels.items():
        status = await ch.status.get()
        print(f"Channel {channel_id}: actuator connected = {status.actuator_plugged}")

    # Multi-channel setpoint/position
    # Note: Using multi_setpoint requires all channels to have an actuator connected 
    # and modulation mode set to SERIAL. Otherwise, the amplifier will ignore the command.
    await device.multi_setpoint.set([10.0, 20.0, 30.0])
    positions = await device.multi_position.get()
finally:
    await device.restore(backup_data)
    await device.close()
```

## Common Patterns

### Connection Pattern

All examples follow this pattern:

``` python
import asyncio
from psj_lib import DDriveDevice, TransportType

async def main():
    # Create device instance
    device = DDriveDevice(TransportType.SERIAL, 'COM3')

    # Connect
    await device.connect()

    try:
        # Use device
        channel = device.channels[0]
        # ... operations ...

    finally:
        # Always disconnect
        await device.disconnect()

if __name__ == "__main__":
    asyncio.run(main())
```

### Error Handling Pattern

Robust error handling:

``` python
from psj_lib import DeviceError

try:
    device = DDriveDevice(TransportType.SERIAL, 'COM3')
    await device.connect()

    # Operations...

except DeviceError as e:
    print(f"Device error: {e}")

finally:
    if device.is_connected:
        await device.disconnect()
```

## Running Examples

### Prerequisites

1.  Install psj-lib:

    ``` bash
    pip install psj-lib
    ```

2.  Connect your d-Drive device via USB or Ethernet

3.  Note the connection details (COM port or IP address)

### Running an Example

``` bash
# Navigate to examples directory
cd examples

# Run example
python 01_device_discovery_and_connection.py
```

**Note**: Adjust the COM port or IP address in each example to match
your setup.

### Modifying Examples

Each example is self-contained and can be modified:

1.  Open example file in your editor
2.  Adjust connection parameters (port, IP address)
3.  Modify parameters (positions, PID values, etc.)
4.  Run and observe results

## Next Steps

- Review full API documentation: `api`
- Learn about advanced features: `advanced_topics`
- Read developer guide: `developer_guide`
- Explore device-specific docs: `d_drive`
