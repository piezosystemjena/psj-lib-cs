# Base Capabilities

This page documents the base capabilities that are available across each
piezo device in psj-lib. These capabilities provide the core
functionality for position control, signal processing, data acquisition,
and system configuration.

Device-specific implementations may extend these base capabilities with
additional features or provide specialized versions. Refer to
device-specific documentation (e.g., `d_drive`) for device-specific
capabilities and enhancements.

## Overview

Capabilities are modular features accessed as properties of device
channels. Each capability provides a focused set of operations for a
specific aspect of device control.

``` python
from psj_lib import DDriveDevice, TransportType

device = DDriveDevice(TransportType.SERIAL, "COM3")
async with device:
    channel = device.channels[0]

    # Access capabilities as channel properties
    await channel.setpoint.set(50.0)
    position = await channel.position.get()
    await channel.pid_controller.set(p=10.0, i=5.0)
```

## Position Control

### Setpoint

**API Reference:**
`~psj_lib.devices.base.capabilities.setpoint.Setpoint`

The setpoint capability controls the target position or voltage for the
actuator. In closed-loop mode, the controller drives the actuator to
match this setpoint. In open-loop mode, the setpoint directly controls
the output voltage.

``` python
# Set target position
await channel.setpoint.set(75.5)

# Read current setpoint
target = await channel.setpoint.get()
print(f"Target: {target:.2f} µm")
```

**Key Points:**

- Units match position units (typically µm in closed-loop, V in
  open-loop)
- Range limited by actuator specifications
- Movement speed affected by slew rate settings

### Position

**API Reference:**
`~psj_lib.devices.base.capabilities.position.Position`

Reads the current position of the actuator. In closed-loop systems, this
represents the sensor feedback value. In open-loop systems, this may
represent the output voltage.

``` python
# Get current position
current_pos = await channel.position.get()
print(f"Position: {current_pos:.2f} µm")

# Calculate position error
target = await channel.setpoint.get()
error = target - current_pos
print(f"Error: {error:.3f} µm")
```

**Key Points:**

- Read-only capability
- Update rate depends on device (typically every control loop cycle)
- Units depend on device configuration

### Closed-Loop Controller

**API Reference:**
`~psj_lib.devices.base.capabilities.closed_loop_controller.ClosedLoopController`

Enables or disables closed-loop position control. When enabled, the
controller uses sensor feedback to actively maintain the actuator at the
desired setpoint, compensating for drift, hysteresis, and external
loads.

``` python
# Enable closed-loop control
await channel.closed_loop_controller.set(True)

# Check control mode
is_closed_loop = await channel.closed_loop_controller.get_enabled()
print(f"Mode: {'Closed-loop' if is_closed_loop else 'Open-loop'}")

# Get controller sampling period
period_us = channel.closed_loop_controller.sample_period
print(f"Control rate: {1e6/period_us:.0f} Hz")
```

**Key Points:**

- Requires position sensor for feedback
- Better accuracy and stability than open-loop
- PID parameters affect closed-loop performance
- Changing modes may cause position jumps

### Slew Rate

**API Reference:**
`~psj_lib.devices.base.capabilities.slew_rate.SlewRate`

Controls the maximum rate of change for actuator movement. Slew rate
limiting prevents mechanical shock, reduces vibration, and protects
delicate samples.

``` python
# Set gentle slew rate for smooth motion
await channel.slew_rate.set(10.0)

# Query current rate
rate = await channel.slew_rate.get()
print(f"Max speed: {rate:.1f} V/ms")
```

**Key Points:**

- Units typically V/ms or %/ms (device-specific)
- Lower values = smoother, slower movements
- Zero or maximum may disable rate limiting (device-specific)

## Control System

### PID Controller

**API Reference:**
`~psj_lib.devices.base.capabilities.pid_controller.PIDController`

Configures PID (Proportional-Integral-Derivative) controller parameters
for closed-loop operation. The PID controller determines how the system
responds to position errors.

``` python
# Set PID parameters
await channel.pid_controller.set(
    p=10.0,      # Proportional gain
    i=5.0,       # Integral gain  
    d=0.5,       # Derivative gain
    diff_filter=100.0  # Derivative filter
)

# Read individual parameters
p_gain = await channel.pid_controller.get_p()
i_gain = await channel.pid_controller.get_i()
print(f"PID: P={p_gain}, I={i_gain}")
```

**Key Points:**

- **P (Proportional):** Response proportional to error. Higher = faster
  but may overshoot
- **I (Integral):** Eliminates steady-state error. Too high causes
  oscillation
- **D (Derivative):** Dampens oscillation. Higher = more damping but
  noise sensitive
- **Diff Filter:** Filters derivative term to reduce noise amplification
- Only active when closed-loop control is enabled
- Improper tuning can cause poor performance

### Pre-Control Factor (PCF)

**API Reference:**
`~psj_lib.devices.base.capabilities.pcf.PreControlFactor`

The Pre-Control Factor provides feedforward compensation to improve
control system response. It anticipates required control action based on
setpoint changes, reducing settling time and tracking error.

``` python
# Set moderate feedforward
await channel.pcf.set(0.5)

# Query current value
value = await channel.pcf.get()
print(f"PCF: {value}")
```

**Key Points:**

- Typical range: 0.0 (no feedforward) to 1.0 (full feedforward)
- Higher values = faster response but potential overshoot
- Only active in closed-loop mode
- Tune in conjunction with PID parameters

## Signal Filtering

### Notch Filter

**API Reference:**
`~psj_lib.devices.base.capabilities.notch_filter.NotchFilter`

Notch filters suppress specific frequency components to eliminate
mechanical resonances that can cause instability or oscillation in
closed-loop systems.

``` python
# Suppress 500 Hz resonance
await channel.notch_filter.set(
    enabled=True,
    frequency=500.0,
    bandwidth=50.0
)

# Check configuration
freq = await channel.notch_filter.get_frequency()
bw = await channel.notch_filter.get_bandwidth()
enabled = await channel.notch_filter.get_enabled()
print(f"Notch: {freq}±{bw/2} Hz, {'On' if enabled else 'Off'}")
```

**Key Points:**

- Center frequency should match mechanical resonance
- Narrow bandwidth = precise suppression
- Wide bandwidth = broader suppression, affects more frequencies

### Low-Pass Filter

**API Reference:**
`~psj_lib.devices.base.capabilities.low_pass_filter.LowPassFilter`

Low-pass filters attenuate high-frequency noise while allowing
low-frequency signals to pass. This improves signal quality and reduces
noise in position measurements or control output.

``` python
# Enable 100 Hz low-pass filter
await channel.lpf.set(
    enabled=True,
    cutoff_frequency=100.0
)

# Check settings
freq = await channel.lpf.get_cutoff_frequency()
enabled = await channel.lpf.get_enabled()
print(f"LPF: {freq} Hz, {'Active' if enabled else 'Bypassed'}")
```

**Key Points:**

- Lower cutoff = more filtering, slower response
- Higher cutoff = less filtering, faster response
- Adds phase lag proportional to filtering strength

### Error Low-Pass Filter

**API Reference:**
`~psj_lib.devices.base.capabilities.error_low_pass_filter.ErrorLowPassFilter`

Applies low-pass filtering specifically to the position error signal
(setpoint - position) before it enters the PID controller. This reduces
high-frequency noise that could cause unstable control behavior.

``` python
# Configure 2nd-order error filter
await channel.error_lpf.set(
    cutoff_frequency=200.0,
    order=2
)

# Query settings
freq = await channel.error_lpf.get_cutoff_frequency()
order = await channel.error_lpf.get_order()
print(f"{order}-order error filter at {freq} Hz")
```

**Key Points:**

- Only affects closed-loop control
- Higher order = steeper rolloff, more phase lag
- Helps stabilize noisy systems
- Coordinate with PID tuning for best stability

## Data Acquisition

### Data Recorder

**API Reference:**
`~psj_lib.devices.base.capabilities.data_recorder.DataRecorder`

The data recorder captures device signals (position, setpoint, voltage,
etc.) at high speed into device memory for later retrieval and analysis.

``` python
recorder = channel.data_recorder

# Configure for 10000 samples, no decimation
await recorder.set(memory_length=10000, stride=1)

# Start recording
await recorder.start()

# ... perform motion or measurements ...

# Retrieve data with progress callback
def progress(current, total):
    print(f"Downloaded {current}/{total} samples")

from psj_lib import DataRecorderChannel
data = await recorder.get_all_data(
    DataRecorderChannel.CHANNEL_1,
    callback=progress
)
print(f"Captured {len(data)} samples")

# Check recorder specifications
sample_rate = recorder.sample_rate  # Hz
sample_period = recorder.sample_period  # microseconds
print(f"Recording at {sample_rate/1000:.0f} kHz")
```

**Key Points:**

- Multiple channels (device-dependent, typically 2)
- Memory length limits total capture time
- Stride (decimation) allows longer time spans at lower data rate
- Large data transfers may take several seconds
- Use `sample_rate` property to get base recording frequency

### Trigger Output

**API Reference:**
`~psj_lib.devices.base.capabilities.trigger_out.TriggerOut`

Generates digital trigger pulses when monitored signals cross threshold
values. Useful for synchronizing external equipment (cameras, data
acquisition, etc.) with actuator movement.

``` python
from psj_lib import TriggerEdge, TriggerDataSource

# Trigger every 10µm from 20µm to 80µm
await channel.trigger_out.set(
    start_value=20.0,
    stop_value=80.0,
    interval=10.0,
    length=100,  # Pulse duration in cycles
    edge=TriggerEdge.BOTH,
    src=TriggerDataSource.POSITION
)

# Query configuration
start = await channel.trigger_out.get_start_value()
interval = await channel.trigger_out.get_interval()
print(f"Trigger every {interval}µm from {start}µm")
```

**Key Points:**

- Output typically 0V/5V TTL signal
- Window mode: triggers when signal enters/exits range
- Interval mode: periodic triggers at fixed spacing
- Edge sensitivity: rising, falling, or both

## Signal Generation

### Static Waveform Generator

**API Reference:**
`~psj_lib.devices.base.capabilities.static_waveform_generator.StaticWaveformGenerator`

Generates continuous periodic waveforms (sine, square, triangle, etc.)
for scanning applications, vibration testing, frequency response
characterization, and dynamic positioning.

``` python
wfg = channel.waveform_generator

# Generate 10 Hz sine wave, 20µm amplitude, centered at 50µm
await wfg.set(
    frequency=10.0,
    amplitude=20.0,
    offset=50.0
)

# Create square wave with 30% duty cycle
await wfg.set(
    frequency=5.0,
    duty_cycle=30.0
)

# Query current settings
freq = await wfg.get_frequency()
amp = await wfg.get_amplitude()
offset = await wfg.get_offset()
print(f"{freq} Hz, ±{amp/2} µm around {offset} µm")
```

**Key Points:**

- Configurable frequency, amplitude, offset, and duty cycle
- May require modulation source selection to use waveform output
- Frequency limited by device capabilities and actuator resonance
- Amplitude limited by actuator travel range

> [!NOTE]
> Device-specific implementations may provide enhanced waveform
> generators with additional waveform types and features. See
> device-specific documentation for details.

## System Monitoring

### Status Register

**API Reference:** `~psj_lib.devices.base.capabilities.status.Status`

Queries the device status register to retrieve real-time hardware state
information, error conditions, and operational flags. The status
register format is device-specific.

``` python
status = await channel.status_register.get()

# Access device-specific status properties
print(f"Raw status: {status.raw}")

# Device-specific implementations provide interpreted properties
# (see device-specific documentation)
```

**Key Points:**

- Status format is device-specific
- Provides real-time device state information
- Device implementations decode raw status into meaningful properties

### Temperature

**API Reference:**
`~psj_lib.devices.base.capabilities.temperature.Temperature`

Monitors the internal temperature of device electronics or power stages.
Temperature monitoring helps prevent overheating and can be used for
thermal management.

``` python
temp = await channel.temperature.get()
print(f"Device temperature: {temp:.1f}°C")

if temp > 60:
    print("Warning: High temperature")
```

**Key Points:**

- Temperature typically in degrees Celsius
- Sensor location varies by device (electronics, power stage)
- Use for thermal monitoring and diagnostics

### Fan Control

**API Reference:** `~psj_lib.devices.base.capabilities.fan.Fan`

Enables or disables the internal cooling fan for thermal management. The
fan helps dissipate heat from power electronics during operation.

``` python
# Enable cooling fan
await channel.fan.set(True)

# Check fan status
is_running = await channel.fan.get_enabled()
print(f"Fan: {'On' if is_running else 'Off'}")
```

**Key Points:**

- Not all devices have controllable fans
- Some fans run automatically based on temperature
- Disabling may cause thermal shutdown under heavy load

## Device Information

### Actuator Description

**API Reference:**
`~psj_lib.devices.base.capabilities.actuator_description.ActuatorDescription`

Retrieves a human-readable description of the piezoelectric actuator
connected to a channel. This may include model number, specifications,
or identifying information.

``` python
desc = await channel.actuator_description.get()
print(f"Connected actuator: {desc}")
# Example output: "MIPOS 100"
```

**Key Points:**

- Description format is actuator-specific
- May include model, travel range, resolution
- Some devices return empty string if not configured

### Unit

**API Reference:** `~psj_lib.devices.base.capabilities.unit.Unit`

Queries the measurement unit for a specific quantity exposed by the
mapped command (for example open-loop voltage unit or closed-loop
position unit).

``` python
unit = await channel.openloop_unit.get()
print(f"Open-loop unit: {unit}")
# Example output: "V"
```

**Key Points:**

- Returns a single unit string for the configured command mapping
- Typical values include `V`, `µm` and `mrad`

### Limits

**API Reference:** `~psj_lib.devices.base.capabilities.limits.Limits`

Reads lower/upper limits for a device quantity (for example voltage or
position limits).

``` python
lower = await channel.openloop_limits.get_lower()
upper = await channel.openloop_limits.get_upper()
print(f"Allowed range: {lower} .. {upper}")
```

**Key Points:**

- Provides read-only range boundaries
- `get_range()` returns `(lower, upper)`

### Display

**API Reference:** `~psj_lib.devices.base.capabilities.display.Display`

Controls device display brightness (when supported).

``` python
await device.display.set(brightness=40.0)
```

### Multi-Channel Helpers

**API References:**

- `~psj_lib.devices.base.capabilities.multi_setpoint.MultiSetpoint`
- `~psj_lib.devices.base.capabilities.multi_position.MultiPosition`

These capabilities allow setting or reading multiple channels
synchronously on devices that expose group operations (for example
NV40/3 variants).

``` python
await device.multi_setpoint.set([10.0, 20.0, 30.0])
positions = await device.multi_position.get()
```

## Signal Routing

### Modulation Source

**API Reference:**
`~psj_lib.devices.base.capabilities.modulation_source.ModulationSource`

Configures which signal source is used to modulate the actuator position
or voltage. Common sources include external analog input, internal
waveform generator, or serial commands.

``` python
# Device-specific enum (example for d-Drive)
from psj_lib import DDriveModulationSourceTypes

# Use internal waveform generator
await channel.modulation_source.set_source(
    DDriveModulationSourceTypes.INTERNAL_WAVEFORM
)

# Check current source
source = await channel.modulation_source.get_source()
print(f"Modulation from: {source.name}")
```

**Key Points:**

- Source enum is device-specific
- External input typically 0-10V
- May need to enable modulation mode separately

### Monitor Output

**API Reference:**
`~psj_lib.devices.base.capabilities.monitor_output.MonitorOutput`

Configures which internal signal is routed to the device's analog
monitor output connector. This allows real-time observation using an
oscilloscope or data acquisition system.

``` python
# Device-specific enum (example for d-Drive)
from psj_lib import DDriveMonitorOutputSource

# Route position to monitor output
await channel.monitor_output.set_source(
    DDriveMonitorOutputSource.POSITION
)

# Check current source
source = await channel.monitor_output.get_source()
print(f"Monitoring: {source.name}")
```

**Key Points:**

- Output typically 0-10V
- Scaling depends on device and selected source
- Source enum is device-specific
- Useful for debugging and real-time monitoring

## Configuration Management

### Factory Reset

**API Reference:**
`~psj_lib.devices.base.capabilities.factory_reset.FactoryReset`

Resets the device to factory default settings. This restores all
parameters (PID, filters, control modes, etc.) to their original values.

``` python
# IMPORTANT: Backup first!
backup = await device.backup()

# Reset to factory defaults
await device.factory_reset.execute()
print("Device reset to factory defaults")
```

**Key Points:**

- **All custom settings are permanently lost**
- Use `device.backup()` to save configuration first
- Cannot be undone

> [!WARNING]
> Factory reset is irreversible. Always backup your configuration before
> performing a factory reset.

## See Also

- `d_drive` - d-Drive specific capabilities and enhancements
- `api` - Complete API reference
- `examples` - Usage examples and tutorials
