![header](docs/images/piezosystem_logo.svg)

# psj-lib

[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/language-C%23-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![Docs](https://img.shields.io/badge/docs-docfx-success)](docs/)

A comprehensive C#/.NET library for controlling piezoelectric amplifiers and control devices manufactured by [piezosystem jena GmbH](https://www.piezosystem.com).

This repository is the C# port of [piezosystemjena/psj-lib](https://github.com/piezosystemjena/psj-lib) and mirrors the same core device model and capability-based API design.

## Features

- **Asynchronous Architecture** - Built on .NET `async`/`await` for efficient, non-blocking device communication
- **Multi-Device Support** - Extensible framework supporting multiple device families (d-Drive, 30DV50/300, and NV variants in this port)
- **Comprehensive Capabilities** - Full access to position control, PID tuning, waveform generation, data recording, and filtering
- **Multiple Transport Protocols** - Connect via Serial (USB) or Telnet (Ethernet)
- **Type-Safe API** - Strongly typed classes and enums for excellent IDE autocomplete
- **Extensive Documentation** - Integrated documentation pages and runnable examples

## Supported Devices

**Note**: For NV200 please use [nv200-python-lib](https://github.com/piezosystemjena/nv200-python-lib).

### d-Drive Modular Amplifier

The d-Drive series represents piezosystem jena's modular piezo amplifier family:

- **High Resolution**: Precision positioning and control
- **Modular Design**: Multi-channel configurations
- **Advanced Control**: Integrated PID controller with configurable filters
- **Waveform Generation**: Built-in function generator
- **Data Acquisition**: High-speed recorder support

### PSJ 30DV50/300 (Standalone Amplifier)

The **PSJ 30DV50/300** is a single-channel, d-Drive-compatible amplifier:

- **Single Channel**: Standalone unit with one channel (ID 0)
- **d-Drive Compatible**: Uses the same command set and capabilities
- **Full Feature Set**: PID control, waveform generation, data recorder, filters

### NV Family (Implemented in This Port)

- **NV120 / NV120CLE**
- **NV403 / NV403CLE**

## Installation

### Using NuGet (Recommended)

```bash
dotnet add package PsjLib
```

### From Source

```bash
git clone https://github.com/piezosystemjena/psj-lib-cs.git
cd psj-lib-cs
dotnet restore
dotnet build
```

### Requirements

- .NET 8.0 SDK or higher
- Windows 10/11, Linux, or macOS

## Quick Start

### Basic Position Control

```csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

// Connect over serial to the amplifier.
var device = new DDriveDevice(TransportType.Serial, "COM3");
await device.ConnectAsync().ConfigureAwait(false);

try
{
	// Use channel 0 for single-channel examples.
	var channel = device.Channels[0];

	// Enable feedback control for precise positioning.
	await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);

	// Command a target setpoint.
	await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);

	// Read back the measured position.
	var position = await channel.Position.GetAsync().ConfigureAwait(false);
	Console.WriteLine($"Position: {position:F2} um");
}
finally
{
	// Always close the device connection.
	await device.CloseAsync().ConfigureAwait(false);
}
```

### Device Discovery

```csharp
using PsjLib.Base;
using PsjLib.Transport;

// Scan for reachable serial devices.
var devices = await PiezoDevice
	.DiscoverDevicesAsync<PiezoDevice>(DiscoverFlags.DetectSerial)
	.ConfigureAwait(false);

// Print basic transport info for each discovered device.
foreach (var device in devices)
{
	var info = device.DeviceInfo;
	Console.WriteLine($"Found: {info.DeviceId} on {info.TransportInfo.Identifier}");
}
```

### PID Control Configuration

```csharp
// Set closed-loop PID gains.
await channel.PidController.SetAsync(
	p: 10.0,
	i: 5.0,
	d: 0.5
).ConfigureAwait(false);

// Enable a notch filter to suppress a known resonance.
await channel.Notch.SetAsync(
	enabled: true,
	frequency: 500.0,
	bandwidth: 50.0
).ConfigureAwait(false);
```

### Waveform Generation

```csharp
using PsjLib.DDriveFamily.Capabilities;

// Configure sine waveform parameters.
await channel.WaveformGenerator.Sine
	.SetAsync(amplitude: 20.0, offset: 20.0, frequency: 5.0)
	.ConfigureAwait(false);

// Activate sine output mode.
await channel.WaveformGenerator
	.SetWaveformTypeAsync(DDriveWaveformType.Sine)
	.ConfigureAwait(false);
```

### Data Recording

```csharp
using PsjLib.DDriveFamily.Capabilities;

// Configure recorder buffer and decimation.
await channel.DataRecorder.SetAsync(
	memoryLength: 50000,
	stride: 1).ConfigureAwait(false);

// Retrieve captured position samples.
var data = await channel.DataRecorder
	.GetAllDataAsync(DDriveDataRecorderChannel.Position, 50000)
	.ConfigureAwait(false);
```

## Architecture

`psj-lib-cs` uses a three-layer hierarchical architecture:

```text
PiezoDevice (e.g., DDriveDevice)
  +- Transport protocol (Serial/Telnet) with command handling
  +- PiezoChannels (e.g., DDriveChannel)
	  +- Capabilities (Position, PID, WaveformGenerator, etc.)
```

### Key Design Patterns

- **Capability-Based Architecture**: Features are modular capability classes
- **Async/Await**: I/O operations use asynchronous APIs
- **Type Safety**: Strong typing for IDE support and reliability

## Documentation

Comprehensive documentation is available [here](https://piezosystemjena.github.io/psj-lib-cs/):

- **Getting Started** - Tutorials and basic usage
- **API Reference** - Library API documentation
- **Device Documentation** - Device-specific guides
- **Base Capabilities** - Common capability behavior
- **Examples** - Practical usage examples
- **Developer Guide** - Extending the library

### Building Documentation Locally

```bash
cd docs
docfx docfx.json --serve
```

Then open `http://localhost:8080` in your browser.

## Examples

The [examples/](examples/) directory contains practical examples:

1. **Device Discovery and Connection** - Finding and connecting to devices
2. **Simple Position Control** - Basic open-loop and closed-loop positioning
3. **PID Tuning** - Controller parameter tuning
4. **Data Recorder Capture** - High-speed data acquisition
5. **Waveform Generation Basics** - Signal generation and scanning
6. **Filter Configuration** - Notch and low-pass filter setup
7. **Backup and Restore Configuration** - Saving/loading device settings
8. **NV403CLE Capabilities Overview** - NV workflow and multi-channel operations

Run an example:

```bash
dotnet run --project examples/Examples.csproj -- 01
```

## Development

### Setup Development Environment

```bash
# Clone repository
git clone https://github.com/piezosystemjena/psj-lib-cs.git
cd psj-lib-cs

# Restore and build
dotnet restore
dotnet build
```

### Library Version

Version metadata is exposed via `PsjLib.LibraryVersion.Version`.

## Support

- **Documentation**: [GitHub Pages](https://piezosystemjena.github.io/psj-lib-cs/)
- **Issues**: [GitHub Issues](https://github.com/piezosystemjena/psj-lib-cs/issues)
- **Website**: [piezosystem jena GmbH](https://www.piezosystem.com)
