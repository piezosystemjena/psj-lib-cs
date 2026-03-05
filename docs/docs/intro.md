# Introduction

![image](images/psj-lib-header.png)

## Welcome to psj-lib

**psj-lib** is a comprehensive C# library for controlling
piezoelectric amplifiers and control devices manufactured by
[piezosystem jena GmbH](https://www.piezosystem.com). The library
provides an intuitive, asynchronous interface for precise position
control, waveform generation, data acquisition, and advanced control
system configuration.

For a Python port of psj-lib, see [psj-lib Python GitHub](https://github.com/piezosystemjena/psj-lib)

## What is psj-lib?

psj-lib is designed to provide researchers, engineers, and developers
with a powerful yet easy-to-use C# interface for piezosystem jena
devices. The library abstracts the complexity of low-level device
communication while exposing the full capabilities of the hardware.

**Key Features:**

- **Asynchronous Architecture**: Built on .NET async/await for
  efficient, non-blocking device communication
- **Multi-Device Support**: Extensible framework supporting d-Drive,
  30DV50/300, and selected NV-series devices
- **Comprehensive Capabilities**: Full access to position control, PID
  tuning, waveform generation, data recording, and filtering
- **Multiple Transport Protocols**: Connect via Serial (USB) or Telnet
  (Ethernet)
- **Type-Safe API**: Complete type hints for excellent IDE autocomplete
  and type checking
- **Extensive Documentation**: Comprehensive docstrings, examples, and
  developer guides

## Supported Devices

### d-Drive Modular Amplifiers

The **d-Drive** series represents piezosystem jena's modular piezo
amplifier family, offering:

- **High Resolution**: 20-bit DAC/ADC for precision control
- **Fast Sampling**: 50 kHz (20 µs period) for responsive control
- **Modular Design**: 1-6 channel configurations in compact enclosure
- **Advanced Control**: Integrated PID controller with configurable
  filters
- **Waveform Generation**: Built-in function generator with scan modes
- **Data Acquisition**: 2-channel recorder with 500,000 samples per
  channel
- **Hardware Triggers**: Precise timing and synchronization

### 30DV50/300 Standalone Amplifier 

The **30DV50/300** is a single-channel amplifier designed for compact
setups that only require one axis. It supports the same command set and
capabilities as d-Drive channels, including PID control, waveform
generation, and data recording.

### NV40/3(CLE) and NV120/1(CLE) Amplifiers

Piezo amplifiers with 1 or 3 channels, available in open-loop and
closed-loop variants. They include features such as:

- **Position Control**: Synchronous setpoint and position reading for
  multiple axes
- **Closed-Loop Control**: Integrated analog closed-loop controller (CLE
  variants)
- **Physical Knob**: Front-panel knob for manual control and control
  mode switching
- **Display**: Front-panel display for real-time position and status
  information

## Architecture Overview

The psj-lib library follows a hierarchical architecture:

``` text
PiezoDevice (Base Class)
├── Device-level operations
├── Channel discovery
└── Transport protocol management
    │
    └── PiezoChannel (Base Class)
        ├── Channel-level operations
        └── Capability management
            │
            └── Capabilities (Features)
                ├── Status and monitoring
                ├── Position control
                ├── Closed-loop controller
                ├── PID tuning
                ├── Filter configuration
                ├── Waveform generation
                └── Data recording
```

**Three-Layer Design:**

1.  **Device Layer**: Manages device connection, discovery, and channel
    enumeration
2.  **Channel Layer**: Provides access to individual amplifier channels
3.  **Capability Layer**: Exposes specific hardware features as
    composable capabilities

## Design Philosophy

### Capability-Based Architecture

Instead of a monolithic device class, psj-lib uses **capabilities** to
represent hardware features. Each capability is a focused module that
encapsulates related functionality:

``` csharp
await channel.ClosedLoopController.SetAsync(true).ConfigureAwait(false);
await channel.Setpoint.SetAsync(50.0).ConfigureAwait(false);
await channel.PidController.SetAsync(p: 0.5, i: 0.1, d: 0.05).ConfigureAwait(false);
```

This design provides:

- **Clarity**: Clear separation of concerns
- **Discoverability**: Easy to explore available features via
  autocomplete
- **Maintainability**: Easy to extend with new capabilities
- **Type Safety**: Each capability has proper type hints

### Asynchronous by Default

All device communication is asynchronous, enabling:

- **Concurrent Operations**: Control multiple devices simultaneously
- **Non-blocking**: Keep UI responsive while waiting for device
  responses
- **Efficient**: Minimize idle time in complex sequences
- **Scalable**: Handle multiple devices without threading complexity

``` csharp
await Task.WhenAll(
  channel1.Setpoint.SetAsync(30.0),
  channel2.Setpoint.SetAsync(60.0),
  channel3.Setpoint.SetAsync(90.0)
).ConfigureAwait(false);
```

### Transport Abstraction

The library abstracts transport protocols, allowing seamless switching
between Serial and Telnet:

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var serialDevice = new DDriveDevice(TransportType.Serial, "COM3");
var telnetDevice = new DDriveDevice(TransportType.Telnet, "192.168.1.100");
```

The same API works regardless of transport type.

## Getting Started

Ready to start using psj-lib? Here's what to do next:

1.  **Installation**: See [Installation](installation.md) for setup instructions
2.  **Connecting**: Learn how to connect to your device in [Connecting](connecting.md)
3.  **Basic Usage**: Follow the tutorial in [Getting Started](getting_started.md)
4.  **Examples**: Explore practical examples in [Examples](examples.md)
5.  **API Reference**: Browse the complete API in [API Reference](api.md)

## Community and Support

**Documentation**: You're reading it! This documentation covers
everything from basic usage to advanced topics.

**Examples**: The library includes multiple examples demonstrating
common tasks.

**Developer Guide**: See [Developer Guide](developer_guide.md) if you want to extend the
library or add support for new devices.

**Contact**: For support, contact [piezosystem jena
GmbH](https://www.piezosystem.com) or [create an issue on GitHub](https://github.com/piezosystemjena/psj-lib-cs/issues/new)

## License and Attribution

psj-lib is developed and maintained by piezosystem jena GmbH.

**Authors**: piezosystem jena GmbH

**.NET Target Framework**: .NET 8+

## What's Next?

Continue to [Installation](installation.md) to set up psj-lib and start controlling your
piezosystem jena devices.
