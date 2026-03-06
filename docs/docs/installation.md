# Installation

This guide covers how to install PsjLib and set up your development
environment.

## Requirements

**.NET Target Framework**

PsjLib requires .NET 8:

``` bash
dotnet --version
# Should show 8.x or higher
```

**Operating Systems**

PsjLib is cross-platform and works on:

- Windows 10/11
- Linux (Ubuntu, Debian, Fedora, etc.)
- macOS 10.15+

**Hardware Requirements**

- For Serial (USB) connection: USB port and appropriate USB-to-Serial
  driver (depends on OS and device)
- For Telnet connection: Ethernet interface and network connectivity
- Piezosystem jena device (e.g., d-Drive amplifier or 30DV50/300)

## Installation Methods

### Method 1: Install from NuGet (Recommended)

Install PsjLib into your project via NuGet:

``` bash
dotnet add package PsjLib
```

This will automatically install all required dependencies.

### Method 2: Install from Source

To install the latest development version from source:

``` bash
# Clone the repository
git clone https://github.com/piezosystemjena/psj-lib-cs.git
cd psj-lib-cs

# Build library
dotnet build PsjLib/PsjLib.csproj
```

## Dependencies

PsjLib targets .NET and uses standard runtime libraries. Device transport and capability handling are implemented in this repository.

**Development tooling (optional):**

- `dotnet` SDK for building/running library and examples
- `docfx` for generating documentation

## Verifying Installation

After installation, verify that PsjLib is installed correctly:

``` csharp
using PsjLib;

Console.WriteLine(LibraryVersion.Version);
// Should print: 1.0.0 (or later)
```

### Check Available Modules

Verify core imports work:

``` csharp
using PsjLib.DDriveFamily;
using PsjLib.Transport;

var device = new DDriveDevice(TransportType.Serial, "COM3");
Console.WriteLine($"Created {device.DeviceId} instance");
```

## Platform-Specific Setup

### Windows Setup

**Serial Driver Installation:**

For USB-based serial communication, ensure proper drivers are installed:

1.  Connect your piezo amplifier via USB
2.  Windows may automatically install drivers
3.  Verify in Device Manager under "Ports (COM & LPT)"
4.  Note the COM port number (e.g., COM3)

### Linux Setup

**Serial Permissions:**

On Linux, you may need to add your user to the `dialout` group for
serial access:

``` bash
sudo usermod -a -G dialout $USER
# Log out and back in for changes to take effect
```

**Check Serial Ports:**

``` bash
ls /dev/ttyUSB* /dev/ttyACM*
# Lists available serial ports
```

### macOS Setup

**Serial Ports:**

macOS typically detects USB-to-Serial devices automatically:

``` bash
ls /dev/cu.* /dev/tty.*
# Lists available serial ports
```

## Setting Up for Development

If you plan to develop with PsjLib or contribute to the project:

### Clone the Repository

``` bash
git clone https://github.com/piezosystemjena/psj-lib-cs.git
cd psj-lib-cs
```

### Install with Development Dependencies

Using NuGet/dotnet CLI (recommended for development):

``` bash
# Restore and build
dotnet restore
dotnet build
```

### Build Documentation Locally

To build and view documentation:

``` bash
cd docs

# Build HTML documentation
docfx docfx.json --serve

# Open "http://localhost:8080" in browser
```

### Running Examples

The `examples/` directory contains ready-to-run example scripts:

``` bash
# Make sure device is connected
dotnet run --project examples/Examples.csproj -- 01
```

## Troubleshooting

### Import Errors

If you get import errors:

``` csharp
CS0246: The type or namespace name 'PsjLib' could not be found
```

**Solution**: Ensure your project references `PsjLib`:

``` bash
dotnet add package PsjLib
dotnet restore
```

### Serial Connection Issues

**Problem**: Cannot connect to device via serial

**Solutions**:

1.  Check cable is properly connected
2.  Verify correct COM port / device path
3.  Ensure no other application is using the port
4.  Check permissions (Linux: add user to `dialout` group)

### Telnet Connection Issues

**Problem**: Cannot connect via Telnet

**Solutions**:

1.  Verify device IP address (check device display or network scan)

2.  Ensure device and computer are on same network

3.  Check firewall settings

4.  Verify Telnet port (default: 23 or device-specific)

5.  Try ping to verify network connectivity:

    ``` bash
    ping 192.168.1.100
    ```

### Dependency Conflicts

**Problem**: Dependency version conflicts

**Solution**: Align package versions and clear local caches if needed:

``` bash
dotnet nuget locals all --clear
dotnet restore --force
```

### Async/Await Compatibility

**Problem**: async/await compile/runtime issues

**Solution**: Ensure you're using .NET 8+ and `Task`-based async correctly:

``` csharp
public static async Task Main()
{
  // Your async code here
}
```

## Getting Help

If you encounter issues not covered here:

1.  Check the [API Reference](api.md) for detailed API documentation
2.  Review [Examples](examples.md) for working code samples
3.  Check existing GitHub issues
4.  Contact piezosystem jena GmbH for support

## Next Steps

Now that PsjLib is installed, you can:

- Learn how to connect to devices: [Connecting](connecting.md)
- Follow the getting started tutorial: [Getting Started](getting_started.md)
- Explore example scripts: [Examples](examples.md)
- Read about d-Drive specifics: [d-Drive](d_drive.md)
