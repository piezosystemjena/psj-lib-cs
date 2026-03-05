# NV40/3(CLE) and NV120/1(CLE)

This page covers the currently supported NV-series amplifiers and their
psj-lib integration.

## Overview

NV series support follows the same three-layer architecture as other
devices:

``` text
NVFamilyDevice
├── Global capabilities (display, knob)
└── NVFamilyChannel / derived channel types
    └── Channel capabilities (setpoint, position, status, monitor output, ...)
```

Shared behavior is implemented in `~psj_lib.NVFamilyDevice` and
`~psj_lib.NVFamilyChannel`, while model-specific classes define channel
count and available closed-loop features.

## Device Variants

### Open-loop variants

- `~psj_lib.devices.nv_family.nv120.nv120_device.NV120Device` (single
  channel)
- `~psj_lib.devices.nv_family.nv403.nv403_device.NV403Device` (three
  channels)

### Closed-loop variants

- `~psj_lib.devices.nv_family.nv120_cle.nv120_cle_device.NV120CLEDevice`
  (single channel)
- `~psj_lib.devices.nv_family.nv403_cle.nv403_cle_device.NV403CLEDevice`
  (three channels)

## Device Capabilities

NV devices provide global (device-level) capabilities that are not tied
to a specific channel:

### User Interface

- **Display**: Front-panel brightness control
- **Knob Configuration**: Encoder mode, timing, acceleration, and step
  behavior

### Multi-Channel Coordination

- **Multi Setpoint**: Set all channel setpoints in one command (NV40/3
  variants)
- **Multi Position**: Read all channel positions in one command (NV40/3
  variants)

## Channel Capabilities

Each NV channel provides a set of capabilities for command/control and
diagnostics:

### Status and Monitoring

- **Status Register**: NV-specific fault and actuator state flags
- **Position**: Actual position readback (voltage for open-loop, sensor
  readback for closed-loop devices)

### Open-Loop Control

- **Setpoint**: Open-loop/closed-loop target setting
- **Open-Loop Unit**: Unit readback for open-loop operation
- **Open-Loop Limits**: Lower and upper admissible range

### Signal Routing

- **Modulation Source**: Select control source (encoder/analog or
  serial)
- **Monitor Output**: Route internal signals to analog monitor output

### Closed-Loop Additions (CLE Variants)

- **Closed-Loop Controller**: Enable/disable closed-loop feedback
- **Closed-Loop Unit**: Unit readback for closed-loop operation
- **Closed-Loop Limits**: Lower and upper admissible range

## Accessing Capabilities

All capabilities are accessed as device or channel attributes. The NV
family provides both standard piezo capabilities and device-specific
implementations.

``` python
from psj_lib import NV403CLEDevice, TransportType

device = NV403CLEDevice(TransportType.SERIAL, "COM10")
async with device:
    channel = device.channels[0]

    # Device-level capability
    await device.display.set(brightness=40.0)

    # Access channel capabilities
    status = await channel.status.get()
    position = await channel.position.get()
    await channel.setpoint.set(25.0)
```

### Device Capabilities Reference

All NV-family device capabilities with API references:

| Property | API Reference | Description |
|----|----|----|
| `display` | `~psj_lib.devices.nv_family.capabilities.nv_display.NVDisplay` | Device display brightness control |
| `knob` | `~psj_lib.devices.nv_family.capabilities.nv_knob.NVKnob`/ `~psj_lib.devices.nv_family.capabilities.nv_knob.NVCLEKnob` (CLE variants) | Encoder knob configuration |
| `multi_setpoint` | `~psj_lib.devices.base.capabilities.multi_setpoint.MultiSetpoint` | Set all channel setpoints synchronously (NV40/3 and NV40/3CLE) |
| `multi_position` | `~psj_lib.devices.base.capabilities.multi_position.MultiPosition` | Read all channel positions synchronously (NV40/3 and NV40/3CLE) |

### Channel Capabilities Reference

All NV-family channel capabilities with API references:

> [!NOTE]
> Some capability readbacks (e.g. setpoint) are cached by the library,
> as NV devices do not provide native readback for these values. Cached
> values are updated on set operations.

<table>
<colgroup>
<col style="width: 25%" />
<col style="width: 35%" />
<col style="width: 40%" />
</colgroup>
<thead>
<tr>
<th>Property</th>
<th>API Reference</th>
<th>Description</th>
</tr>
</thead>
<tbody>
<tr>
<td><code>setpoint</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_setpoint.NVSetpoint</code></td>
<td><div class="line-block">Target open-loop/closed-loop setpoint</div>
<strong>Cached readback</strong></td>
</tr>
<tr>
<td><code>position</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.position.Position</code></td>
<td>Actual channel position readback</td>
</tr>
<tr>
<td><code>modulation_source</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_modulation_source.NVModulationSource</code></td>
<td><div class="line-block">Modulation source selection (expects <code
class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_modulation_source.NVModulationSourceTypes</code>
enum)</div>
<strong>Cached readback</strong></td>
</tr>
<tr>
<td><code>monitor_output</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_monitor_output.NVMonitorOutput</code></td>
<td><div class="line-block">Analog monitor output routing (expects <code
class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_monitor_output.NVMonitorOutputSource</code>
enum)</div>
<strong>Cached readback</strong></td>
</tr>
<tr>
<td><code>openloop_unit</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.unit.Unit</code></td>
<td>Unit of the open-loop command domain</td>
</tr>
<tr>
<td><code>openloop_limits</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.limits.Limits</code></td>
<td>Open-loop lower and upper limits</td>
</tr>
<tr>
<td><code>status</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.status.Status</code></td>
<td>Status access using <code class="interpreted-text"
role="class">~psj_lib.devices.nv_family.capabilities.nv_status_register.NVStatusRegister</code></td>
</tr>
<tr>
<td><code>closed_loop_controller</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.closed_loop_controller.ClosedLoopController</code></td>
<td>Closed-loop feedback control enable/disable (CLE variants only)</td>
</tr>
<tr>
<td><code>closedloop_unit</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.unit.Unit</code></td>
<td>Unit of the closed-loop command domain (CLE variants only)</td>
</tr>
<tr>
<td><code>closedloop_limits</code></td>
<td><code class="interpreted-text"
role="class">~psj_lib.devices.base.capabilities.limits.Limits</code></td>
<td>Closed-loop lower and upper limits (CLE variants only)</td>
</tr>
</tbody>
</table>

## Usage Example

``` python
import asyncio
from psj_lib import NV403CLEDevice, TransportType

async def main():
    device = NV403CLEDevice(TransportType.SERIAL, "COM10")

    async with device:
        await device.display.set(brightness=50.0)

        ch0 = device.channels[0]
        await ch0.closed_loop_controller.set(enabled=True)
        await ch0.setpoint.set(25.0)
        print(await ch0.position.get())

        await device.multi_setpoint.set([10.0, 20.0, 30.0])
        print(await device.multi_position.get())

asyncio.run(main())
```
