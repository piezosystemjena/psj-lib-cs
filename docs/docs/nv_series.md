# NV40/3(CLE) and NV120/1(CLE)

This page covers the currently supported NV-series amplifiers and their
psj-lib integration.

## Overview

NV series support follows the same three-layer architecture as other
devices:

``` text
NVFamilyDevice
├── Global capabilities (Display, Knob)
└── NVFamilyChannel / derived channel types
    └── Channel capabilities (Setpoint, Position, Status, MonitorOutput, ...)
```

Shared behavior is implemented in `PsjLib.NVFamily.NVFamilyDevice` and
`PsjLib.NVFamily.NVFamilyChannel`, while model-specific classes define
channel count and available closed-loop features.

## Device Variants

### Open-loop variants

- [NV120Device](../api/PsjLib.NVFamily.NV120.NV120Device.yml) (single channel)
- [NV403Device](../api/PsjLib.NVFamily.NV403.NV403Device.yml) (three channels)

### Closed-loop variants

- [NV120CLEDevice](../api/PsjLib.NVFamily.NV120CLE.NV120CLEDevice.yml) (single channel)
- [NV403CLEDevice](../api/PsjLib.NVFamily.NV403CLE.NV403CLEDevice.yml) (three channels)

## Device Capabilities

NV devices provide global (device-level) capabilities that are not tied
to a specific channel:

### User Interface

- **Display**: Front-panel brightness control (`device.Display`)
- **Knob Configuration**: Encoder mode, timing, acceleration, and step
  behavior (`device.Knob`)

### Multi-Channel Coordination

- **Multi Setpoint**: Set all channel setpoints in one command
  (`device.MultiSetpoint`, NV40/3 variants)
- **Multi Position**: Read all channel positions in one command
  (`device.MultiPosition`, NV40/3 variants)

> [!NOTE]
> To use the Multi Setpoint capability, all 3 channels must have an actuator connected and
> their modulation source set to "SERIAL". If this is not the case, 
> the amplifier will ignore the command.

## Channel Capabilities

Each NV channel provides a set of capabilities for command/control and
diagnostics:

### Status and Monitoring

- **Status Register**: NV-specific fault and actuator state flags
  (`channel.Status`)
- **Position**: Actual position readback (`channel.Position`)

### Open-Loop Control

- **Setpoint**: Open-loop/closed-loop target setting (`channel.Setpoint`)
- **Open-Loop Unit**: Unit readback for open-loop operation
  (`channel.OpenloopUnit`)
- **Open-Loop Limits**: Lower and upper admissible range
  (`channel.OpenloopLimits`)

### Signal Routing

- **Modulation Source**: Select control source (`channel.ModulationSource`)
- **Monitor Output**: Route internal signals to analog output
  (`channel.MonitorOutput`)

### Closed-Loop Additions (CLE Variants)

- **Closed-Loop Controller**: Enable/disable feedback control
  (`channel.ClosedLoopController`)
- **Closed-Loop Unit**: Unit readback (`channel.ClosedloopUnit`)
- **Closed-Loop Limits**: Range readback (`channel.ClosedloopLimits`)

## Accessing Capabilities

All capabilities are accessed as device or channel properties.

``` csharp
using PsjLib.NVFamily.NV403CLE;
using PsjLib.Transport;

var device = new NV403CLEDevice(TransportType.Serial, "COM10");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    var channel = device.Channels[0];

    await device.Display.SetAsync(40.0).ConfigureAwait(false);

    var status = await channel.Status.GetAsync().ConfigureAwait(false);
    var position = await channel.Position.GetAsync().ConfigureAwait(false);
    await channel.Setpoint.SetAsync(25.0).ConfigureAwait(false);
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Device Capabilities Reference

| Property | API Reference | Description |
|----|----|----|
| `Display` | [NVDisplay](../api/PsjLib.NVFamily.Capabilities.NVDisplay.yml) | Device display brightness control |
| `Knob` | [NVKnob](../api/PsjLib.NVFamily.Capabilities.NVKnob.yml) / [NVCLEKnob](../api/PsjLib.NVFamily.Capabilities.NVCLEKnob.yml) | Encoder knob configuration |
| `MultiSetpoint` | [MultiSetpoint](../api/PsjLib.Base.Capabilities.MultiSetpoint.yml) | Set all channel setpoints synchronously (NV40/3 variants) |
| `MultiPosition` | [MultiPosition](../api/PsjLib.Base.Capabilities.MultiPosition.yml) | Read all channel positions synchronously (NV40/3 variants) |

### Channel Capabilities Reference

> [!NOTE]
> Some readbacks are cached by the library for NV devices (for example
> setpoint/modulation/monitor output), because firmware does not expose
> direct read commands for all values.

| Property | API Reference | Description |
|----|----|----|
| `Setpoint` | [NVSetpoint](../api/PsjLib.NVFamily.Capabilities.NVSetpoint.yml) | Target open-loop/closed-loop setpoint (cached readback) |
| `Position` | [Position](../api/PsjLib.Base.Capabilities.Position.yml) | Actual channel position readback |
| `ModulationSource` | [NVModulationSource](../api/PsjLib.NVFamily.Capabilities.NVModulationSource.yml) | Modulation source selection ([NVModulationSourceTypes](../api/PsjLib.NVFamily.Capabilities.NVModulationSourceTypes.yml), cached readback) |
| `MonitorOutput` | [NVMonitorOutput](../api/PsjLib.NVFamily.Capabilities.NVMonitorOutput.yml) | Analog monitor output routing ([NVMonitorOutputSource](../api/PsjLib.NVFamily.Capabilities.NVMonitorOutputSource.yml), cached readback) |
| `OpenloopUnit` | [Unit](../api/PsjLib.Base.Capabilities.Unit.yml) | Unit of open-loop command domain |
| `OpenloopLimits` | [Limits](../api/PsjLib.Base.Capabilities.Limits.yml) | Open-loop lower/upper limits |
| `Status` | [`Status<TRegister>`](../api/PsjLib.Base.Capabilities.Status-1.yml), [`NVStatusRegister`](../api/PsjLib.NVFamily.Capabilities.NVStatusRegister.yml) | NV status register access |
| `ClosedLoopController` | [ClosedLoopController](../api/PsjLib.Base.Capabilities.ClosedLoopController.yml) | Closed-loop feedback enable/disable (CLE only) |
| `ClosedloopUnit` | [Unit](../api/PsjLib.Base.Capabilities.Unit.yml) | Closed-loop unit (CLE only) |
| `ClosedloopLimits` | [Limits](../api/PsjLib.Base.Capabilities.Limits.yml) | Closed-loop limits (CLE only) |

## Usage Example

``` csharp
using PsjLib.NVFamily.Capabilities;
using PsjLib.NVFamily.NV403CLE;
using PsjLib.Transport;

var device = new NV403CLEDevice(TransportType.Serial, "COM10");
await device.ConnectAsync().ConfigureAwait(false);

try
{
    await device.Display.SetAsync(50.0).ConfigureAwait(false);

    var ch0 = device.Channels[0];
    await ch0.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    await ch0.Setpoint.SetAsync(25.0).ConfigureAwait(false);
    Console.WriteLine(await ch0.Position.GetAsync().ConfigureAwait(false));

    await device.MultiSetpoint.SetAsync(new[] { 10.0, 20.0, 30.0 }).ConfigureAwait(false);
    var positions = await device.MultiPosition.GetAsync().ConfigureAwait(false);
    Console.WriteLine(string.Join(", ", positions));

    foreach (var channel in device.Channels.Values)
    {
        await channel.ModulationSource.SetAsync(NVModulationSourceTypes.Serial).ConfigureAwait(false);
    }
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

**Notes:**

- Some NV values are cached client-side where firmware has no readback command.
- `MultiSetpoint` and `MultiPosition` are available on NV40/3 model variants.
