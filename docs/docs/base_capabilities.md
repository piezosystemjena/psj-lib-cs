# Base Capabilities

This page documents the base capabilities that are available across
piezo devices in PsjLib. These capabilities provide core functionality
for position control, signal processing, data acquisition, and system
configuration.

Device-specific implementations may extend these base capabilities with
additional features or specialized behavior. Refer to
device-specific documentation (for example [d-Drive](d_drive.md)) for details.

## Overview

Capabilities are modular features exposed as properties of devices and channels.

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);
try
{
    var channel = device.Channels[0];
    await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
    var position = await channel.Position.GetAsync().ConfigureAwait(false);
    await channel.PidController.SetAsync(p: 10.0, i: 5.0).ConfigureAwait(false);
}
finally
{
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Position Control

### Setpoint

**API Reference:** [Setpoint](../api/PsjLib.Base.Capabilities.Setpoint.yml)

Controls target position or voltage for the actuator.

``` csharp
await channel.Setpoint.SetAsync(75.5).ConfigureAwait(false);
var target = await channel.Setpoint.GetAsync().ConfigureAwait(false); // cached where applicable
Console.WriteLine($"Target: {target:F2}");
```

**Key Points:**

- Units depend on control mode and device configuration
- Range is limited by hardware and firmware constraints
- Movement dynamics are influenced by slew-rate settings

### Position

**API Reference:** [Position](../api/PsjLib.Base.Capabilities.Position.yml)

Reads actual position (or corresponding readback quantity on open-loop
devices).

``` csharp
var currentPos = await channel.Position.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Position: {currentPos:F2}");
```

**Notes:**

- Position is a readback value from the device, not a cached command value.
- Reported units depend on device mode and channel configuration.

### Closed-Loop Controller

**API Reference:** [ClosedLoopController](../api/PsjLib.Base.Capabilities.ClosedLoopController.yml)

Enables/disables closed-loop feedback control.

``` csharp
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
var enabled = await channel.ClosedLoopController.GetEnabledAsync().ConfigureAwait(false);
Console.WriteLine($"Mode: {(enabled ? "Closed-loop" : "Open-loop")}");
Console.WriteLine($"Sample period: {channel.ClosedLoopController.SamplePeriod} µs");
```

**Notes:**

- Closed-loop availability depends on hardware and connected actuator/sensor.
- Controller timing properties are device-defined and read-only.

### Slew Rate

**API Reference:** [SlewRate](../api/PsjLib.Base.Capabilities.SlewRate.yml)

Limits maximum rate-of-change for smoother motion.

``` csharp
await channel.SlewRate.SetAsync(10.0).ConfigureAwait(false);
var rate = await channel.SlewRate.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Max speed: {rate:F1}");
```

**Notes:**

- Lower slew rates improve smoothness but increase move time.
- Use conservative limits for fragile setups or high-load conditions.

## Control System

### PID Controller

**API Reference:** [PIDController](../api/PsjLib.Base.Capabilities.PIDController.yml)

Configures PID gains and derivative filter (`Tf`).

``` csharp
await channel.PidController.SetAsync(p: 10.0, i: 5.0, d: 0.5, tf: 100.0).ConfigureAwait(false);
var p = await channel.PidController.GetPAsync().ConfigureAwait(false);
var i = await channel.PidController.GetIAsync().ConfigureAwait(false);
var d = await channel.PidController.GetDAsync().ConfigureAwait(false);
var tf = await channel.PidController.GetTfAsync().ConfigureAwait(false);
Console.WriteLine($"PID: P={p}, I={i}, D={d}, Tf={tf}");
```

**Notes:**

- Tune gains incrementally to avoid oscillation or overshoot.
- `Tf` smooths derivative behavior and helps reduce noise sensitivity.

### Pre-Control Factor (PCF)

**API Reference:** [PreControlFactor](../api/PsjLib.Base.Capabilities.PreControlFactor.yml)

Feedforward compensation for faster response.

``` csharp
await channel.Pcf.SetAsync(0.5).ConfigureAwait(false);
var value = await channel.Pcf.GetAsync().ConfigureAwait(false);
Console.WriteLine($"PCF: {value}");
```

**Notes:**

- PCF is feedforward and should be tuned together with PID gains.
- Overly aggressive values can reduce stability margins.

## Signal Filtering

### Notch Filter

**API Reference:** [NotchFilter](../api/PsjLib.Base.Capabilities.NotchFilter.yml)

Suppresses resonance frequencies.

``` csharp
await channel.Notch.SetAsync(enabled: true, frequency: 500.0, bandwidth: 50.0).ConfigureAwait(false);
var notchFreq = await channel.Notch.GetFrequencyAsync().ConfigureAwait(false);
var notchBw = await channel.Notch.GetBandwidthAsync().ConfigureAwait(false);
```

**Notes:**

- Match notch center frequency to measured resonance peaks.
- Keep bandwidth as narrow as practical to avoid excessive phase impact.

### Low-Pass Filter

**API Reference:** [LowPassFilter](../api/PsjLib.Base.Capabilities.LowPassFilter.yml)

Attenuates high-frequency components.

``` csharp
await channel.Lpf.SetAsync(enabled: true, cutoffFrequency: 100.0).ConfigureAwait(false);
var lpfCutoff = await channel.Lpf.GetCutoffFrequencyAsync().ConfigureAwait(false);
```

**Notes:**

- Lower cutoffs improve noise rejection but slow response.
- Validate closed-loop behavior after major cutoff changes.

### Error Low-Pass Filter

**API Reference:** [ErrorLowPassFilter](../api/PsjLib.Base.Capabilities.ErrorLowPassFilter.yml)

Filters error signal before PID processing.

``` csharp
await channel.ErrorLpf.SetAsync(cutoffFrequency: 200.0, order: 2).ConfigureAwait(false);
var order = await channel.ErrorLpf.GetOrderAsync().ConfigureAwait(false);
```

**Notes:**

- Error filtering can stabilize noisy systems before PID processing.
- Higher order filters increase attenuation and phase lag.

## Data Acquisition

### Data Recorder

**API Reference:** [DataRecorder](../api/PsjLib.Base.Capabilities.DataRecorder.yml)

Captures high-speed device data to internal memory.

``` csharp
using PsjLib.DDriveFamily.Capabilities;

var recorder = channel.DataRecorder;
await recorder.SetAsync(memoryLength: 10000, stride: 1).ConfigureAwait(false);

var data = await recorder.GetAllDataAsync(
    DDriveDataRecorderChannel.Position,
    10000,
    (current, total) => Console.WriteLine($"Downloaded {current}/{total}")
).ConfigureAwait(false);

Console.WriteLine($"Sample rate: {recorder.SampleRate:F0} Hz");
```

**Notes:**

- Recorder downloads may take noticeable time for large captures.
- Use progress callbacks for UI feedback during long reads.

### Trigger Output

**API Reference:** [TriggerOut](../api/PsjLib.Base.Capabilities.TriggerOut.yml)

Generates trigger pulses based on signal thresholds/ranges.

``` csharp
using PsjLib.Base.Capabilities;

await channel.TriggerOut.SetAsync(
    startValue: 20.0,
    stopValue: 80.0,
    interval: 10.0,
    length: 100,
    edge: TriggerEdge.Both,
    src: TriggerDataSource.Position
).ConfigureAwait(false);
```

**Notes:**

- Trigger polarity/edge behavior is controlled by `TriggerEdge`.
- Verify electrical compatibility with connected external hardware.

## Signal Generation

### Static Waveform Generator

**API Reference:** [StaticWaveformGenerator](../api/PsjLib.Base.Capabilities.StaticWaveformGenerator.yml)

Base abstraction for periodic waveforms. Device families may expose
derived capability types with additional waveform options.

``` csharp
using PsjLib.DDriveFamily.Capabilities;

await channel.WaveformGenerator.Sine
    .SetAsync(amplitude: 20.0, offset: 50.0, frequency: 10.0)
    .ConfigureAwait(false);
await channel.WaveformGenerator
    .SetWaveformTypeAsync(DDriveWaveformType.Sine)
    .ConfigureAwait(false);
```

**Notes:**

- Configure waveform parameters before enabling output waveform type.
- Ensure amplitude/offset remain within actuator-safe operating range.

## System Monitoring

### Status Register

**API Reference:** [Status<TRegister>](../api/PsjLib.Base.Capabilities.Status-1.yml)

Reads model-specific status registers.

``` csharp
var status = await channel.StatusRegister.GetAsync().ConfigureAwait(false);
Console.WriteLine(status);
```

**Notes:**

- Status fields and bit meanings are model-specific.
- Polling interval should match your monitoring needs and transport budget.

### Temperature

**API Reference:** [Temperature](../api/PsjLib.Base.Capabilities.Temperature.yml)

Reads internal device temperature.

``` csharp
var temp = await channel.Temperature.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Temperature: {temp:F1}°C");
```

**Notes:**

- Use temperature checks during prolonged high-power operation.
- Add your own thermal safety threshold handling in application logic.

### Fan Control

**API Reference:** [Fan](../api/PsjLib.Base.Capabilities.Fan.yml)

Controls cooling fan where supported.

``` csharp
await channel.Fan.SetAsync(true).ConfigureAwait(false);
var fanEnabled = await channel.Fan.GetEnabledAsync().ConfigureAwait(false);
```

**Notes:**

- Fan control availability depends on specific hardware.
- Enabling active cooling can improve thermal headroom.

## Device Information

### Actuator Description

**API Reference:** [ActuatorDescription](../api/PsjLib.Base.Capabilities.ActuatorDescription.yml)

Returns actuator identification text.

``` csharp
var description = await channel.ActuatorDescription.GetAsync().ConfigureAwait(false);
Console.WriteLine(description);
```

**Notes:**

- Description strings are firmware-provided and model dependent.
- Use this value for diagnostics and setup validation.

### Unit

**API Reference:** [Unit](../api/PsjLib.Base.Capabilities.Unit.yml)

Reads unit string for mapped command domains.

``` csharp
var unit = await channel.OpenloopUnit.GetAsync().ConfigureAwait(false);
Console.WriteLine($"Open-loop unit: {unit}");
```

**Notes:**

- Unit mapping depends on current control domain and device family.
- Read units before presenting values in UI/log output.

### Limits

**API Reference:** [Limits](../api/PsjLib.Base.Capabilities.Limits.yml)

Reads lower/upper admissible ranges.

``` csharp
var range = await channel.OpenloopLimits.GetRangeAsync().ConfigureAwait(false);
Console.WriteLine($"Allowed range: {range.Lower} .. {range.Upper}");
```

**Notes:**

- Limits should be checked before issuing movement/output commands.
- Boundaries can differ between open-loop and closed-loop modes.

### Display

**API Reference:** [Display](../api/PsjLib.Base.Capabilities.Display.yml)

Controls front-panel display brightness where available.

``` csharp
await device.Display.SetAsync(40.0).ConfigureAwait(false);
```

**Notes:**

- Display capability may be device-level rather than channel-level.
- Brightness range and behavior are device-specific.

### Multi-Channel Helpers

**API References:**

- [MultiSetpoint](../api/PsjLib.Base.Capabilities.MultiSetpoint.yml)
- [MultiPosition](../api/PsjLib.Base.Capabilities.MultiPosition.yml)

``` csharp
await device.MultiSetpoint.SetAsync(new[] { 10.0, 20.0, 30.0 }).ConfigureAwait(false);
var positions = await device.MultiPosition.GetAsync().ConfigureAwait(false);
```

**Notes:**

- Multi-channel helpers are only available on supported multi-axis models.
- Use these calls for synchronized set/read patterns across channels.

## Signal Routing

### Modulation Source

**API Reference:** [ModulationSource](../api/PsjLib.Base.Capabilities.ModulationSource.yml)

Configures modulation source using device-specific enums.

``` csharp
using PsjLib.DDriveFamily.Capabilities;

await channel.ModulationSource
    .SetAsync(DDriveModulationSourceTypes.SerialEncoder)
    .ConfigureAwait(false);
```

**Notes:**

- Available source enums are family/model specific.
- Some modulation sources require additional external wiring or mode setup.

### Monitor Output

**API Reference:** [MonitorOutput](../api/PsjLib.Base.Capabilities.MonitorOutput.yml)

Routes internal signals to monitor output.

``` csharp
using PsjLib.DDriveFamily.Capabilities;

await channel.MonitorOutput
    .SetAsync(DDriveMonitorOutputSource.ClosedLoopPosition)
    .ConfigureAwait(false);
```

**Notes:**

- Monitor output source enums are family specific.
- Confirm output scaling before quantitative measurements.

## Configuration Management

### Factory Reset

**API Reference:** [FactoryReset](../api/PsjLib.Base.Capabilities.FactoryReset.yml)

Resets device configuration to factory defaults.

``` csharp
var backup = await device.BackupAsync().ConfigureAwait(false);
await device.FactoryReset.ExecuteAsync().ConfigureAwait(false);
```

**Key Points:**

- All custom settings are permanently lost
- Use `device.BackupAsync()` to save configuration first
- Restore with `device.RestoreAsync(backup)` where appropriate

> [!WARNING]
> Factory reset is irreversible. Always back up configuration before
> performing a reset.

## See Also

- [d-Drive](d_drive.md) - d-Drive specific capabilities and enhancements
- [API Reference](api.md) - Complete API reference
- [Examples](examples.md) - Usage examples and tutorials
