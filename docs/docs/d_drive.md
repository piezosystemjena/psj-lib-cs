d-Drive and 30DV50/300
==============

This page covers the d-Drive modular amplifier family, its features, and
how to use it with PsjLib.

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
- **Position**: Actuator position readback
- **Closed-Loop Controller**: Enable/disable feedback control
- **Slew Rate**: Maximum rate-of-change limiting

## Control System

- **PID Controller**: Proportional-Integral-Derivative tuning
- **Pre-Control Factor (PCF)**: Feedforward compensation
- **Notch Filter**: Resonance suppression
- **Low-Pass Filter**: Signal noise reduction
- **Error LPF**: PID error signal filtering

## Signal Generation

- **Waveform Generator**: Function generation (sine, triangle, rectangle,
  noise, sweep)
- **Modulation Source**: External or internal signal modulation
- **Monitor Output**: Configurable analog output routing

## Data Acquisition

- **Data Recorder**: Two-channel data capture (500k samples)
- **Trigger Out**: Hardware trigger generation (TTL output)

# Accessing Capabilities

All capabilities are accessed as channel properties. d-Drive channels
use [DDriveChannel](../api/PsjLib.DDriveFamily.DDriveChannel.yml) (derived from
[DDriveFamilyChannel](../api/PsjLib.DDriveFamily.DDriveFamilyChannel.yml)).

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    var channel = device.Channels[0];
    var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
    var position = await channel.Position.GetAsync().ConfigureAwait(false);
    await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Using PSJ 30DV series

The 30DV50/300 exposes a single channel (ID 0) with the same
capabilities as a d-Drive channel:

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new PSJ30DVDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    var channel = device.Channels[0];
    await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
    await channel.Setpoint.SetAsync(10.0).ConfigureAwait(false);
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Channel Capabilities Reference

All d-Drive channel capabilities with API references:

| Property | API Reference | Description |
|----|----|----|
| `StatusRegister` | [Status<TRegister>](../api/PsjLib.Base.Capabilities.Status-1.yml), [DDriveStatusRegister](../api/PsjLib.DDriveFamily.Capabilities.DDriveStatusRegister.yml) | Hardware status with d-Drive-specific flags |
| `ActuatorDescription` | [ActuatorDescription](../api/PsjLib.Base.Capabilities.ActuatorDescription.yml) | Actuator identification and specifications |
| `Setpoint` | [DDriveSetpoint](../api/PsjLib.DDriveFamily.Capabilities.DDriveSetpoint.yml) | Target position/voltage setpoint (cached readback) |
| `Position` | [Position](../api/PsjLib.Base.Capabilities.Position.yml) | Actual position readback |
| `Temperature` | [Temperature](../api/PsjLib.Base.Capabilities.Temperature.yml) | Amplifier temperature |
| `Fan` | [Fan](../api/PsjLib.Base.Capabilities.Fan.yml) | Cooling fan enable/disable |
| `ClosedLoopController` | [DDriveClosedLoopController](../api/PsjLib.DDriveFamily.Capabilities.DDriveClosedLoopController.yml) | Feedback control enable/disable |
| `SlewRate` | [SlewRate](../api/PsjLib.Base.Capabilities.SlewRate.yml) | Maximum rate-of-change limiting |
| `Pcf` | [PreControlFactor](../api/PsjLib.Base.Capabilities.PreControlFactor.yml) | Feedforward compensation |
| `PidController` | [PIDController](../api/PsjLib.Base.Capabilities.PIDController.yml) | PID configuration (P, I, D, Tf) |
| `Notch` | [NotchFilter](../api/PsjLib.Base.Capabilities.NotchFilter.yml) | Notch filter for resonance suppression |
| `Lpf` | [LowPassFilter](../api/PsjLib.Base.Capabilities.LowPassFilter.yml) | Low-pass filter |
| `ErrorLpf` | [ErrorLowPassFilter](../api/PsjLib.Base.Capabilities.ErrorLowPassFilter.yml) | Error-signal low-pass filter |
| `ModulationSource` | [ModulationSource](../api/PsjLib.Base.Capabilities.ModulationSource.yml) | Modulation source selection ([DDriveModulationSourceTypes](../api/PsjLib.DDriveFamily.Capabilities.DDriveModulationSourceTypes.yml)) |
| `MonitorOutput` | [MonitorOutput](../api/PsjLib.Base.Capabilities.MonitorOutput.yml) | Analog monitor source ([DDriveMonitorOutputSource](../api/PsjLib.DDriveFamily.Capabilities.DDriveMonitorOutputSource.yml)) |
| `WaveformGenerator` | [DDriveWaveformGenerator](../api/PsjLib.DDriveFamily.Capabilities.DDriveWaveformGenerator.yml) | Multi-waveform generator and scan modes |
| `DataRecorder` | [DDriveDataRecorder](../api/PsjLib.DDriveFamily.Capabilities.DDriveDataRecorder.yml) | Two-channel recorder (position/voltage) |
| `TriggerOut` | [DDriveTriggerOut](../api/PsjLib.DDriveFamily.Capabilities.DDriveTriggerOut.yml) | Trigger output with offset support |

## d-Drive Status Register

`DDriveStatusRegister` provides d-Drive-specific hardware state:

``` csharp
var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Actuator plugged: {status.ActorPlugged}");
Console.WriteLine($"Sensor type: {status.SensorType}");
Console.WriteLine($"Voltage enabled: {status.PiezoVoltageEnabled}");
Console.WriteLine($"Closed-loop: {status.ClosedLoop}");
Console.WriteLine($"Active waveform: {status.WaveformGeneratorStatus}");
Console.WriteLine($"Notch filter: {status.NotchFilterActive}");
Console.WriteLine($"Low-pass filter: {status.LowPassFilterActive}");
```

**Notes:**

- Status values are parsed from firmware register bits and are read-only.
- Flag naming mirrors the C# API model for predictable diagnostics code.

## d-Drive Waveform Generator

`DDriveWaveformGenerator` supports `DDriveWaveformType.None`,
`Sine`, `Triangle`, `Rectangle`, `Noise`, and `Sweep`.

**Basic Usage:**

``` csharp
using PsjLib.DDriveFamily.Capabilities;

var wfg = channel.WaveformGenerator;
await wfg.Sine.SetAsync(amplitude: 20.0, offset: 50.0, frequency: 10.0).ConfigureAwait(false);
await wfg.SetWaveformTypeAsync(DDriveWaveformType.Sine).ConfigureAwait(false);
await wfg.SetWaveformTypeAsync(DDriveWaveformType.None).ConfigureAwait(false);
```

**Automated Scan Function:**

``` csharp
using PsjLib.DDriveFamily.Capabilities;

await wfg.StartScanAsync(DDriveScanType.TriangleOnce).ConfigureAwait(false);
while (await wfg.IsScanRunningAsync().ConfigureAwait(false))
{
    await Task.Delay(100).ConfigureAwait(false);
}
```

**Notes:**

- Always set waveform parameters before activating waveform type.
- Use `DDriveWaveformType.None` to stop generator output cleanly.

## d-Drive Data Recorder

`DDriveDataRecorder` records two channels simultaneously.

**Basic Usage:**

``` csharp
using PsjLib.DDriveFamily.Capabilities;

var recorder = channel.DataRecorder;
await recorder.SetAsync(memoryLength: 50000, stride: 1).ConfigureAwait(false);

var positionData = await recorder.GetAllDataAsync(
    DDriveDataRecorderChannel.Position,
    50000).ConfigureAwait(false);
var voltageData = await recorder.GetAllDataAsync(
    DDriveDataRecorderChannel.Voltage,
    50000).ConfigureAwait(false);
```

**Notes:**

- Recorder channel mapping is fixed by firmware (position/voltage).
- Choose `memoryLength`/`stride` based on required capture duration.

**d-Drive Data Format:**

- **Position channel**: Percent of full closed-loop range
- **Voltage channel**: Output voltage in volts

## d-Drive Closed-Loop Controller

`DDriveClosedLoopController` extends base closed-loop handling and reads
status from the d-Drive status register.

``` csharp
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
var isEnabled = await channel.ClosedLoopController.GetEnabledAsync().ConfigureAwait(false);
var samplePeriodUs = channel.ClosedLoopController.SamplePeriod; // 20 µs
var sampleRateHz = channel.ClosedLoopController.SampleRate;     // 50 kHz
```

**Notes:**

- Closed-loop state is obtained from the d-Drive status register.
- Control-loop timing is fixed by hardware and exposed as read-only properties.

## d-Drive Setpoint

`DDriveSetpoint.GetAsync()` returns the cached setpoint value.

``` csharp
await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
var target = await channel.Setpoint.GetAsync().ConfigureAwait(false);  // cached
var actual = await channel.Position.GetAsync().ConfigureAwait(false);   // measured
var error = target - actual;
Console.WriteLine($"Position error: {error:F3} µm");
```

**Notes:**

- `GetAsync()` returns the client-side cached setpoint.
- Use `Position.GetAsync()` for measured hardware feedback.

# Multi-Channel Coordination

d-Drive devices support 1-6 channels. Use parallel operations to keep
control loops aligned across channels.

``` csharp
await Task.WhenAll(device.Channels.Values.Select(ch =>
    ch.ClosedLoopController.SetAsync(true))).ConfigureAwait(false);

var targets = new[] { 30.0, 50.0, 70.0 };
await Task.WhenAll(device.Channels.Values.Zip(targets).Select(x =>
    x.First.Setpoint.SetAsync(x.Second))).ConfigureAwait(false);
```

# Next Steps

Explore detailed capability documentation:

- **Getting Started**: [Getting Started](getting_started.md) - Basic device control and
  position feedback
- **Base Capabilities**: [Base Capabilities](base_capabilities.md) - Control system,
  filtering, data acquisition, and signal generation
- **Examples**: [Examples](examples.md) - Real-world use cases and complete
  applications
