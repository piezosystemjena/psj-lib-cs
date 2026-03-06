# Examples

This page demonstrates common use cases with complete, working examples.

## Example Scripts

All examples are available in the `examples/` directory.

### 01 - Device Discovery and Connection

Demonstrates device discovery, connection, and basic information
retrieval.

**What You'll Learn:**

- Discovering devices on selected interfaces
- Connecting to a discovered device
- Displaying `DeviceInfo`
- Listing available channels

**Code:** `examples/01_device_discovery_and_connection.cs`

``` csharp
using PsjLib.Base;
using PsjLib.DDriveFamily;
using PsjLib.Transport;

// Search for D-Drive devices on serial interfaces.
var devices = await PiezoDevice
    .DiscoverDevicesAsync<DDriveDevice>(DiscoverFlags.DetectSerial)
    .ConfigureAwait(false);

// Print discovered device metadata.
foreach (var found in devices)
{
    Console.WriteLine(found.DeviceInfo);
}
```

### 02 - Simple Position Control

Basic position control with open-loop and closed-loop modes.

**What You'll Learn:**

- Reading status/position
- Setting target positions
- Open-loop vs closed-loop control
- Position error checking

**Code:** `examples/02_simple_position_control.cs`

``` csharp
// Enable closed-loop control so setpoints are tracked by position feedback.
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

// Define target positions in micrometers.
var targets = new[] { 30.0, 50.0, 70.0 };

// Move to each target and verify the resulting position error.
foreach (var target in targets)
{
    await channel.Setpoint.SetAsync(target).ConfigureAwait(false);

    // Wait briefly for the system to settle.
    await Task.Delay(1000).ConfigureAwait(false);

    var actual = await channel.Position.GetAsync().ConfigureAwait(false);
    var error = Math.Abs(actual - target);
    Console.WriteLine($"Target: {target:F1} µm, Actual: {actual:F2} µm, Error: {error:F3} µm");
}
```

### 03 - PID Tuning

Configure and test PID controller parameters.

**What You'll Learn:**

- Setting P, I, D values
- Reading current PID configuration
- Testing simple step response workflow

**Code:** `examples/03_pid_tuning.cs`

``` csharp
// Apply new PID gains.
await channel.PidController.SetAsync(p: 0.5, i: 0.1, d: 0.05).ConfigureAwait(false);

// Read back the active PID gains from the device.
var p = await channel.PidController.GetPAsync().ConfigureAwait(false);
var i = await channel.PidController.GetIAsync().ConfigureAwait(false);
var d = await channel.PidController.GetDAsync().ConfigureAwait(false);

// Log the configuration for verification.
Console.WriteLine($"PID Parameters: P={p:F3}, I={i:F3}, D={d:F3}");
```

### 04 - Data Recorder Capture

Record position and voltage data and export CSV.

**What You'll Learn:**

- Configuring memory length and stride
- Retrieving recorder data with progress callback
- Saving data to CSV

**Code:** `examples/04_data_recorder_capture.cs`

``` csharp
using PsjLib.DDriveFamily.Capabilities;

// Configure recorder memory and sampling stride.
await channel.DataRecorder.SetAsync(memoryLength: 1000, stride: 1).ConfigureAwait(false);

// Read the full position trace from recorder channel memory.
var positionData = await channel.DataRecorder.GetAllDataAsync(
    DDriveDataRecorderChannel.Position,
    1000).ConfigureAwait(false);
```

### 05 - Waveform Generation Basics

Generate periodic waveforms for scanning and testing.

**What You'll Learn:**

- Configuring sine/triangle/sweep waveforms
- Switching active waveform types
- Stopping waveform output

**Code:** `examples/05_waveform_generation_basics.cs`

``` csharp
using PsjLib.DDriveFamily.Capabilities;

// Configure sine waveform output parameters.
await channel.WaveformGenerator.Sine
    .SetAsync(amplitude: 20.0, offset: 20.0, frequency: 5.0)
    .ConfigureAwait(false);

// Activate sine waveform mode.
await channel.WaveformGenerator
    .SetWaveformTypeAsync(DDriveWaveformType.Sine)
    .ConfigureAwait(false);
```

### 06 - Filter Configuration

Configure notch filter, LPF, and error LPF for stable control.

**What You'll Learn:**

- Enabling/configuring notch filter
- Setting low-pass cutoff
- Error low-pass filter setup

**Code:** `examples/06_filter_configuration.cs`

``` csharp
// Enable and tune notch filter around the resonance.
await channel.Notch.SetAsync(enabled: true, frequency: 500.0, bandwidth: 50.0).ConfigureAwait(false);

// Enable low-pass filtering on the control path.
await channel.Lpf.SetAsync(enabled: true, cutoffFrequency: 100.0).ConfigureAwait(false);

// Smooth control error with a second-order low-pass filter.
await channel.ErrorLpf.SetAsync(cutoffFrequency: 200.0, order: 2).ConfigureAwait(false);
```

### 07 - Backup and Restore Configuration

Save and restore device configuration.

**What You'll Learn:**

- Backing up device/channel configuration
- Saving backup map to JSON
- Restoring configuration from memory/file

**Code:** `examples/07_backup_and_restore_configuration.cs`

``` csharp
// Snapshot current device configuration.
var backup = await device.BackupAsync().ConfigureAwait(false);

// Restore configuration from the backup map.
await device.RestoreAsync(backup).ConfigureAwait(false);
```

### 08 - NV403CLE Capabilities Overview

Demonstrates key NV403CLE capabilities in one workflow.

**What You'll Learn:**

- Backing up configuration and restoring at end
- Setting modulation source to serial for all channels
- Reading/writing display brightness
- Toggling closed-loop control
- Reading open-loop/closed-loop units and limits
- Using `MultiSetpoint` and `MultiPosition`

**Code:** `examples/08_nv403cle_capabilities_overview.cs`

``` csharp
using PsjLib.NVFamily.Capabilities;
using PsjLib.NVFamily.NV403CLE;
using PsjLib.Transport;

// Connect to NV403CLE over serial.
var device = new NV403CLEDevice(TransportType.Serial, "COM10");
await device.ConnectAsync().ConfigureAwait(false);

// Save current configuration so we can restore it later.
var backup = await device.BackupAsync().ConfigureAwait(false);

try
{
    // Route modulation control to serial commands on all channels.
    foreach (var ch in device.Channels.Values)
    {
        await ch.ModulationSource.SetAsync(NVModulationSourceTypes.Serial).ConfigureAwait(false);
    }

    // Apply setpoints to all channels in one command.
    await device.MultiSetpoint.SetAsync(new[] { 10.0, 20.0, 30.0 }).ConfigureAwait(false);

    // Read back all channel positions.
    var positions = await device.MultiPosition.GetAsync().ConfigureAwait(false);
}
finally
{
    // Always restore original state and close the connection.
    await device.RestoreAsync(backup).ConfigureAwait(false);
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Common Patterns

### Connection Pattern

All examples follow this pattern:

``` csharp
// Create device instance with connection settings.
var device = new DDriveDevice(TransportType.Serial, "COM3");

// Open transport and initialize capabilities.
await device.ConnectAsync().ConfigureAwait(false);
try
{
    // Access first channel and perform operations.
    var channel = device.Channels[0];
    // operations
}
finally
{
    // Ensure transport is closed even on errors.
    await device.CloseAsync().ConfigureAwait(false);
}
```

### Error Handling Pattern

``` csharp
using PsjLib.Base;

try
{
    // Attempt to connect to the device.
    await device.ConnectAsync().ConfigureAwait(false);
}
catch (DeviceError ex)
{
    // Handle library-specific device exceptions.
    Console.WriteLine($"Device error: {ex.Message}");
}
finally
{
    // Always release the connection.
    await device.CloseAsync().ConfigureAwait(false);
}
```

## Running Examples

### Prerequisites

1. Build the solution:

    ``` bash
    dotnet build
    ```

2. Connect your device via USB or Ethernet.

3. Update COM port/IP in the selected example if needed.

### Running an Example

``` bash
dotnet run --project examples/Examples.csproj -- 01
```

### Modifying Examples

1. Open the corresponding `.cs` file in `examples/`
2. Adjust connection settings and parameters
3. Run again with `dotnet run --project examples/Examples.csproj -- XX`

## Next Steps

- Review full API documentation: [API Reference](api.md)
- Read developer guide: [Developer Guide](developer_guide.md)
- Explore device-specific docs: [d-Drive](d_drive.md)
