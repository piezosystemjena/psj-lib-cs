d-Drive and 30DV50/300 ==============

This page covers the d-Drive modular amplifier family, its features, and
how to use it with psj-lib.

# Overview

The **d-Drive** is piezosystem jena's modular piezo amplifier series,
designed for precision nanopositioning and high-dynamic control
applications. Each d-Drive unit can contain 1-6 independent amplifier
channels in a compact enclosure.

The **30DV50/300** is a single-channel amplifier that shares the same
command set and capability model as d-Drive. It is ideal for compact,
single-axis setups.

# Hardware Specifications

## Digital Control System

- **Resolution**: 20-bit DAC and ADC (1,048,576 steps)
- **Sample Rate**: 50 kHz (20 µs control loop period)
- **Control Loop**: Digital PID with feedforward compensation (PCF)

# Channel Capabilities

Each d-Drive channel provides comprehensive control capabilities:

## Status and Monitoring

- **Status Register**: Real-time hardware state flags
- **Temperature**: Internal amplifier temperature monitoring
- **Actuator Description**: Connected piezo description readout
- **Fan Control**: Active cooling fan management

## Position Control

- **Setpoint**: Voltage and position target setting
- **Position**: Actor position readback
- **Closed-Loop Controller**: Enable/disable feedback control
- **Slew Rate**: Maximum rate of change limiting

## Control System

- **PID Controller**: Proportional-Integral-Derivative tuning
- **Pre-Control Factor (PCF)**: Feedforward compensation
- **Notch Filter**: Resonance suppression
- **Low-Pass Filter**: Signal noise reduction
- **Error LPF**: PID error signal filtering

## Signal Generation

- **Waveform Generator**: Function generation (sine, triangle, sweep,
  etc.)
- **Modulation Source**: External or internal signal modulation
- **Monitor Output**: Configurable analog output routing

## Data Acquisition

- **Data Recorder**: Two-channel data capture (500k samples)
- **Trigger Out**: Hardware trigger generation (TTL output)

# Accessing Capabilities

All capabilities are accessed as channel attributes. The d-Drive
provides both standard piezo capabilities and device-specific
implementations.

``` python
from psj_lib import DDriveDevice, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]

    # Access any capability as a channel property
    status = await channel.status_register.get()
    position = await channel.position.get()
    await channel.setpoint.set(50.0)
```

## Using PSJ 30DV series

The 30DV50/300 exposes a single channel (ID 0) with the same
capabilities as a d-Drive channel:

``` python
from psj_lib import PSJ30DVDevice, TransportType

device = PSJ30DVDevice(TransportType.SERIAL, "COM3")
async with device:
  channel = device.channels[0]
  await channel.closed_loop_controller.set(True)
  await channel.setpoint.set(10.0)
```

## Channel Capabilities Reference

All d-Drive channel capabilities with API references:

| Property | API Reference | Description |
|----|----|----|
| `status_register` | `~psj_lib.devices.base.capabilities.status.StatusCapability` | Hardware status with d-Drive specific flags (actuator detection, sensor type, waveform status) |
| `actuator_description` | `~psj_lib.devices.base.capabilities.actuator_description.ActuatorDescription` | Actuator identification and specifications |
| `setpoint` | `~psj_lib.devices.base.capabilities.setpoint.Setpoint` | Target position control (write-only, readback is cached client-side) |
| `position` | `~psj_lib.devices.base.capabilities.position.Position` | Actual position readback (read-only, updates every 500ms) |
| `temperature` | `~psj_lib.devices.base.capabilities.temperature.Temperature` | Amplifier electronics temperature monitoring |
| `fan` | `~psj_lib.devices.base.capabilities.fan.Fan` | Cooling fan enable/disable control (Presence of fan is hardware dependent) |
| `closed_loop_controller` | `~psj_lib.devices.base.capabilities.closed_loop_controller.ClosedLoopController` | Feedback control enable/disable |
| `slew_rate` | `~psj_lib.devices.base.capabilities.slew_rate.SlewRate` | Maximum rate of change limiting |
| `pcf` | `~psj_lib.devices.base.capabilities.pcf.PreControlFactor` | Pre-control factor (feedforward compensation) |
| `pid_controller` | `~psj_lib.devices.base.capabilities.pid_controller.PIDController` | PID controller configuration (P, I, D, diff filter) |
| `notch` | `~psj_lib.devices.base.capabilities.notch_filter.NotchFilter` | Notch filter for resonance suppression |
| `lpf` | `~psj_lib.devices.base.capabilities.low_pass_filter.LowPassFilter` | Low-pass filter for signal conditioning |
| `error_lpf` | `~psj_lib.devices.base.capabilities.error_low_pass_filter.ErrorLowPassFilter` | Error signal low-pass filter |
| `modulation_source` | `~psj_lib.devices.base.capabilities.modulation_source.ModulationSource` | Modulation input source selection (expects `~psj_lib.devices.d_drive_family.capabilities.d_drive_modulation_source.DDriveModulationSourceTypes` enum) |
| `monitor_output` | `~psj_lib.devices.base.capabilities.monitor_output.MonitorOutput` | Analog monitor output routing (expects `~psj_lib.devices.d_drive_family.capabilities.d_drive_monitor_output.DDriveMonitorOutputSource` enum) |
| `waveform_generator` | `~psj_lib.devices.d_drive_family.capabilities.d_drive_waveform_generator.DDriveWaveformGenerator` | Multi-waveform generator with 5 types (sine, triangle, rectangle, noise, sweep) and automated scan function |
| `data_recorder` | `~psj_lib.devices.base.capabilities.data_recorder.DataRecorder` | Two-channel recorder: position + voltage, 500k samples max, 50 kHz sample rate |
| `trigger_out` | `~psj_lib.devices.d_drive_family.capabilities.d_drive_trigger_out.DDriveTriggerOut` | Hardware trigger output with d-Drive specific offset parameter |

## d-Drive Status Register

The
`~psj_lib.devices.d_drive_family.capabilities.d_drive_status_register.DDriveStatusRegister`
provides d-Drive specific hardware state information:

``` python
from psj_lib import DDriveDevice, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]
    status = await channel.status_register.get()

    # d-Drive specific status flags
    print(f"Actuator plugged: {status.actor_plugged}")
    print(f"Sensor type: {status.sensor_type.name}")
    print(f"Voltage enabled: {status.piezo_voltage_enabled}")
    print(f"Closed-loop: {status.closed_loop}")
    print(f"Active waveform: {status.waveform_generator_status.name}")
    print(f"Notch filter: {status.notch_filter_active}")
    print(f"Low-pass filter: {status.low_pass_filter_active}")
```

**Key d-Drive Status Flags:**

- `actor_plugged`: Actuator physically connected and detected
- `sensor_type`: Position sensor type (`~psj_lib.SensorType` enum)
- `piezo_voltage_enabled`: High voltage output enabled
- `closed_loop`: Closed-loop feedback control active
- `waveform_generator_status`: Active waveform type
  (`~psj_lib.DDriveWaveformGeneratorStatus` enum)
- `notch_filter_active`: Notch filter enabled status
- `low_pass_filter_active`: Low-pass filter enabled status

## d-Drive Waveform Generator

The
`~psj_lib.devices.d_drive_family.capabilities.d_drive_waveform_generator.DDriveWaveformGenerator`
provides 5 waveform types and automated scanning:

**Waveform Types**
(`~psj_lib.devices.d_drive_family.capabilities.d_drive_waveform_generator.DDriveWaveformType`):

- `SINE`: Sinusoidal waveform
- `TRIANGLE`: Triangular waveform with adjustable duty cycle
- `RECTANGLE`: Square/rectangular waveform with duty cycle
- `NOISE`: Random noise for dithering
- `SWEEP`: Linear sweep/ramp (time in seconds for full cycle)

**Basic Usage:**

``` python
from psj_lib import DDriveDevice, DDriveWaveformType, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]
    wfg = channel.waveform_generator

    # Configure sine wave
    await wfg.sine.set(
        amplitude=20.0,   # 20 µm peak-to-peak
        offset=50.0,      # Center at 50 µm
        frequency=10.0    # 10 Hz
    )

    # Activate sine waveform
    await wfg.set_waveform_type(DDriveWaveformType.SINE)

    # Stop waveform
    await wfg.set_waveform_type(DDriveWaveformType.NONE)
```

**Automated Scan Function:**

The d-Drive supports automated single or double scan cycles:

``` python
from psj_lib import DDriveScanType

# Start automated single triangle scan
await wfg.start_scan(DDriveScanType.TRIANGLE_ONCE)

# Check if scan is still running
while await wfg.is_scan_running():
    await asyncio.sleep(0.1)

print("Scan completed")
```

**Scan Types**
(`~psj_lib.devices.d_drive_family.capabilities.d_drive_waveform_generator.DDriveScanType`):

- `SINE_ONCE`: Single sine cycle
- `TRIANGLE_ONCE`: Single triangle cycle (up and down)
- `SINE_TWICE`: Two sine cycles
- `TRIANGLE_TWICE`: Two triangle cycles
- `OFF`: No automated scan

See `base_capabilities` for base waveform generator documentation.

## d-Drive Data Recorder

The `~psj_lib.devices.base.capabilities.data_recorder.DataRecorder`
records two channels simultaneously at up to 50 kHz:

**Channel Mapping:**

- **Channel 1**: Position sensor signal
- **Channel 2**: Actuator voltage

**Key Specifications:**

- Maximum 500,000 samples per channel
- 50 kHz sample rate (20 µs period)
- Stride (decimation) support for longer duration at lower rate
- Both channels always record same length

**Basic Usage:**

``` python
from psj_lib import DDriveDevice, DDriveDataRecorderChannel, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]
    recorder = channel.data_recorder

    # Configure for 1 second at 50 kHz
    await recorder.set(
        memory_length=50000,  # 50k samples at 50 kHz = 1 sec
        stride=1              # No decimation
    )

    # Start recording
    await recorder.start()

    # ... perform motion or waveform ...

    # Retrieve data
    position_data = await recorder.get_all_data(
        DDriveDataRecorderChannel.POSITION
    )
    voltage_data = await recorder.get_all_data(
        DDriveDataRecorderChannel.VOLTAGE
    )
```

See `base_capabilities` for base data recorder documentation.

**d-Drive Data Format:**

The d-Drive recorder returns data that is automatically parsed by
`~psj_lib.devices.d_drive_family.capabilities.d_drive_data_recorder.DDriveDataRecorder`:

- **Position Channel**: Returns percentage of full closed-loop motion
  range (includes ±30% overshoot capability)
- **Voltage Channel**: Returns output voltage in Volts

**Configuration Notes:**

- `get_memory_length()` returns 500000 (maximum hardware capacity)
- `get_stride()` returns 0 (d-Drive doesn't support reading this back)

## d-Drive Closed-Loop Controller

The
`~psj_lib.devices.d_drive_family.capabilities.d_drive_closed_loop_controller.DDriveClosedLoopController`
extends the base controller with d-Drive specific status reading:

``` python
from psj_lib import DDriveDevice, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]
    controller = channel.closed_loop_controller

    # Enable closed-loop control
    await controller.set(True)

    # Check status via status register (d-Drive specific)
    is_enabled = await controller.get_enabled()
    print(f"Closed-loop active: {is_enabled}")

    # Get control loop frequency
    period = controller.sample_period  # 20 µs for d-Drive
    frequency = 1000000 / period  # 50 kHz
    print(f"Control loop: {frequency:.0f} Hz")
```

**Key Features:**

- Reads closed-loop state from hardware status register (bit 7)
- 50 kHz control loop (20 µs sample period)
- Integrated with PID controller for precise position control

## d-Drive Setpoint

The
`~psj_lib.devices.d_drive_family.capabilities.d_drive_setpoint.DDriveSetpoint`
provides setpoint control with client-side caching:

``` python
from psj_lib import DDriveDevice, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]

    # Set target position
    await channel.setpoint.set(50.0)  # 50 µm

    # Read back cached value (not from hardware)
    target = await channel.setpoint.get()
    print(f"Target: {target} µm")

    # Compare with actual position
    actual = await channel.position.get()
    error = target - actual
    print(f"Position error: {error:.3f} µm")
```

**Important Notes:**

- `get()` returns the **cached** value, not a hardware read
- d-Drive hardware does not support reading back setpoint
- Cache is updated only when `set()` is called
- If setpoint is changed by another application, cache will be stale
- Initial cache value is 0.0 before first `set()` call

# Multi-Channel Coordination

d-Drive devices support 1-6 channels. Use parallel operations for
efficient multi-channel control:

``` python
from psj_lib import DDriveDevice, TransportType
import asyncio

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    # Enable closed-loop on all channels in parallel
    await asyncio.gather(*[
        ch.closed_loop_controller.set(True)
        for ch in device.channels
    ])

    # Move all channels simultaneously
    positions = [30.0, 50.0, 70.0]
    await asyncio.gather(*[
        ch.setpoint.set(pos)
        for ch, pos in zip(device.channels, positions)
    ])
```

# Next Steps

Explore detailed capability documentation:

- **Getting Started**: `getting_started` - Basic device control and
  position feedback
- **Base Capabilities**: `base_capabilities` - Control system,
  filtering, data acquisition, and signal generation
- **Examples**: `examples` - Real-world use cases and complete
  applications
