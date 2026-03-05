# API Reference

Complete API reference for psj-lib.

## Quick Navigation

**Core Components**

- [Devices](#devices) - Device and channel classes
- [Base Capabilities](#base-capabilities) - Common capabilities across
  all devices
- [NV Family](#nv-family) - NV-series device and capability classes
- [d-Drive Specific Capabilities](#d-drive-specific-capabilities) -
  d-Drive enhanced features
- [Exceptions](#exceptions) - Error handling
- [Type Definitions](#type-definitions) - Type aliases and enums

**Base Capabilities by Category**

- [Status and Monitoring](#status-and-monitoring) - Status register,
  temperature, actuator info
- [Position Control](#position-control) - Setpoint, position,
  closed-loop control, slew rate
- [Control System](#control-system) - PID controller, pre-control factor
- [Filters](#filters) - Notch filter, low-pass filters, error filtering
- [Signal Generation](#signal-generation) - Modulation source, monitor
  output, waveform generation
- [Data Acquisition](#data-acquisition) - Data recorder, trigger output
- [Configuration](#configuration) - Unit, limits, display, factory
  reset, fan control

**Usage Patterns**

- [Common Patterns](#common-patterns) - Quick reference for typical
  operations
- [Async Patterns](#async-patterns) - Async/await usage examples
- [Type Hints](#type-hints) - Type annotation support

## Package Structure

``` text
psj_lib/
├── devices/
│   ├── base/                    # Base device classes (public: PiezoDevice, PiezoChannel)
│   │   ├── piezo_device.py
│   │   ├── piezo_channel.py
│   │   ├── exceptions.py
│   │   └── capabilities/        # Base capabilities (all public)
│   ├── d_drive_family/          # d-Drive family (d-Drive + 30DV series)
│   │   ├── d_drive/             # d-Drive modular system
│   │   │   ├── d_drive_device.py
│   │   │   └── d_drive_channel.py
│   │   ├── psj_30dv/             # PSJ 30DV single-channel device
│   │   │   ├── psj_30dv_device.py
│   │   │   └── psj_30dv_channel.py
│   │   └── capabilities/        # d-Drive family capabilities (all public)
│   │       ├── d_drive_status_register.py
│   │       ├── d_drive_waveform_generator.py
│   │       └── ...
│   ├── nv_family/               # NV family (NV120/NV403, OL + CLE variants)
│   │   ├── nv_family_device.py
│   │   ├── nv_family_channel.py
│   │   ├── nv120/
│   │   ├── nv120_cle/
│   │   ├── nv403/
│   │   └── nv403_cle/
│   └── transport_protocol/      # Internal (only TransportType, DiscoverFlags, TransportProtocolInfo exported)
└── _internal/                   # Internal utilities
```

**Note**: Only classes and types exported in `psj_lib.__init__.py` are
part of the public API. Internal modules like `device_factory` and
`transport_protocol` implementation details should not be accessed
directly by end users.

## Quick Reference

### Main Classes

``` python
from psj_lib import DDriveDevice, TransportType, DiscoverFlags
```

## Core Modules

### Devices

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices

</div>

### Base Device Classes

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.piezo_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.piezo_channel

</div>

### d-Drive Family

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.d_drive_family_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.d_drive_family_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.d_drive.d_drive_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.d_drive.d_drive_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.psj_30dv.psj_30dv_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.psj_30dv.psj_30dv_channel

</div>

### NV Family

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv_family_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv_family_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv120.nv120_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv120.nv120_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv120_cle.nv120_cle_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv120_cle.nv120_cle_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv403.nv403_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv403.nv403_channel

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv403_cle.nv403_cle_device

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.nv403_cle.nv403_cle_channel

</div>

### Base Capabilities

#### Status and Monitoring

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.status

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.temperature

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.actuator_description

</div>

#### Position Control

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.position

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.setpoint

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.closed_loop_controller

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.slew_rate

</div>

#### Control System

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.pid_controller

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.pcf

</div>

#### Filters

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.notch_filter

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.low_pass_filter

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.error_low_pass_filter

</div>

#### Signal Generation

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.modulation_source

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.monitor_output

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.static_waveform_generator

</div>

#### Data Acquisition

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.data_recorder

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.trigger_out

</div>

#### Configuration

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.unit

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.limits

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.display

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.multi_setpoint

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.multi_position

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.factory_reset

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.capabilities.fan

</div>

### d-Drive Specific Capabilities

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_status_register

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_waveform_generator

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_data_recorder

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_trigger_out

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_modulation_source

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.d_drive_family.capabilities.d_drive_monitor_output

</div>

### NV Specific Capabilities

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_display

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_knob

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_modulation_source

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_monitor_output

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_setpoint

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.nv_family.capabilities.nv_status_register

</div>

### Exceptions

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.exceptions

</div>

### Type Definitions

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.base.piezo_types

</div>

<div class="automodule" members="" undoc-members="" show-inheritance="">

psj_lib.devices.transport_protocol.transport_types

</div>

**Note**: `TransportType`, `DiscoverFlags`, and `TransportProtocolInfo`
are exported from the main `psj_lib` module, not from
`transport_protocol` directly.

## Common Patterns

### Device Creation

``` python
# Direct instantiation (recommended)
device = DDriveDevice(TransportType.SERIAL, "COM3")

# From discovery (devices are already instantiated)
devices = await DDriveDevice.discover_devices()
device = devices[0]  # Already a DDriveDevice instance
```

### Connection Management

``` python
# Context manager (recommended)
async with device:
    # Use device
    pass

# Manual
await device.open()
try:
    # Use device
    pass
finally:
    await device.close()
```

### Channel Access

``` python
# Get all channels
channels = device.channels

# Access by index
channel = device.channels[0]

# Iterate
for channel in device.channels:
    # Use channel
    pass
```

### Capability Access

``` python
# Capabilities are channel attributes
position = channel.position
setpoint = channel.setpoint
pid = channel.pid_controller
recorder = channel.data_recorder

# Use capabilities
await setpoint.set(50.0)
await pid.set(p=0.5, i=0.1, d=0.05)
await recorder.start()
```

### Error Handling

``` python
from psj_lib import DeviceError, DeviceUnavailableException

try:
    async with device:
        await channel.setpoint.set(50.0)

except DeviceUnavailableException as e:
    print(f"Connection failed: {e}")

except DeviceError as e:
    print(f"Device error: {e}")
```

## Type Hints

psj-lib includes comprehensive type hints for IDE autocomplete:

``` python
from psj_lib import DDriveDevice, TransportType, Position, PIDController

async def typed_function(device: DDriveDevice) -> float:
    channel = device.channels[0]

    # IDE provides autocomplete for all methods
    position: Position = channel.position
    value: float = await position.get()

    return value
```

## Async Patterns

### Sequential Operations

``` python
# Operations execute one after another
await channel.setpoint.set(30.0)
await channel.setpoint.set(50.0)
await channel.setpoint.set(70.0)
```

### Parallel Operations

``` python
# Operations execute concurrently
await asyncio.gather(
    channel1.setpoint.set(30.0),
    channel2.setpoint.set(60.0),
    channel3.setpoint.set(90.0)
)
```

### Timeouts

``` python
# With timeout
async with asyncio.timeout(5.0):
    await channel.setpoint.set(50.0)
```

## Index

- `genindex`
- `modindex`
- `search`
