# Installation

This guide covers how to install psj-lib and set up your development
environment.

## Requirements

**Python Version**

psj-lib requires Python 3.12:

``` bash
python --version
# Should show Python 3.12.x or higher
```

**Operating Systems**

psj-lib is cross-platform and works on:

- Windows 10/11
- Linux (Ubuntu, Debian, Fedora, etc.)
- macOS 10.15+

**Hardware Requirements**

- For Serial (USB) connection: USB port and appropriate USB-to-Serial
  driver (depends on OS and device)
- For Telnet connection: Ethernet interface and network connectivity
- Piezosystem jena device (e.g., d-Drive amplifier or 30DV50/300)

## Installation Methods

### Method 1: Install from PyPI (Recommended)

Once published, you can install psj-lib directly from PyPI:

``` bash
pip install psj-lib
```

This will automatically install all required dependencies.

### Method 2: Install with Poetry

If you're using Poetry for dependency management:

``` bash
poetry add psj-lib
```

### Method 3: Install from Source

To install the latest development version from source:

``` bash
# Clone the repository
git clone https://github.com/piezosystemjena/psj-lib.git
cd psj-lib

# Install with pip
pip install -e .

# Or with Poetry
poetry install
```

## Dependencies

psj-lib automatically installs the following dependencies:

**Core Dependencies:**

- `aioserial >= 1.3.1`: Asynchronous serial communication
- `telnetlib3 >= 2.0.4`: Asynchronous Telnet client
- `psutil >= 7.0.0`: System utilities (for port detection)
- `scipy >= 1.16.0`: Scientific computing utilities

**Development Dependencies** (optional):

- `sphinx >= 8.2.3`: Documentation generation
- `sphinx-rtd-theme >= 3.0.2`: Documentation theme
- `matplotlib >= 3.10.1`: Plotting for examples

## Verifying Installation

After installation, verify that psj-lib is installed correctly:

``` python
import psj_lib
print(psj_lib.__version__)
# Should print: 0.0.1 (or later)
```

### Check Available Modules

Verify core imports work:

``` python
from psj_lib import DDriveDevice, TransportType

print("All imports successful!")
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

If you plan to develop with psj-lib or contribute to the project:

### Clone the Repository

``` bash
git clone https://github.com/piezosystemjena/psj-lib.git
cd psj-lib
```

### Install with Development Dependencies

Using Poetry (recommended for development):

``` bash
# Install Poetry if not already installed
pip install poetry

# Install project with all dependencies
poetry install
```

### Build Documentation Locally

To build and view documentation:

``` bash
cd doc

# Build HTML documentation
poetry run sphinx-build -b html . _build/

# Open in browser (Windows)
start _build/html/index.html

# Open in browser (Linux)
xdg-open _build/html/index.html

# Open in browser (macOS)
open _build/html/index.html
```

### Running Examples

The `examples/` directory contains ready-to-run example scripts:

``` bash
# Make sure device is connected
python examples/01_device_discovery_and_connection.py
```

## Troubleshooting

### Import Errors

If you get import errors:

``` python
ModuleNotFoundError: No module named 'psj_lib'
```

**Solution**: Ensure psj-lib is installed in your current Python
environment:

``` bash
pip list | grep psj-lib
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

**Solution**: Use a virtual environment to isolate dependencies:

``` bash
python -m venv psj_env
source psj_env/bin/activate  # or psj_env\Scripts\activate on Windows
pip install psj-lib
```

### AsyncIO Compatibility

**Problem**: asyncio errors or event loop issues

**Solution**: Ensure you're using Python 3.12+ and asyncio correctly:

``` python
import asyncio

async def main():
    # Your async code here
    pass

# Correct way to run
asyncio.run(main())
```

## Getting Help

If you encounter issues not covered here:

1.  Check the `api` reference for detailed API documentation
2.  Review `examples` for working code samples
3.  Check existing GitHub issues
4.  Contact piezosystem jena GmbH for support

## Next Steps

Now that psj-lib is installed, you can:

- Learn how to connect to devices: `connecting`
- Follow the getting started tutorial: `getting_started`
- Explore example scripts: `examples`
- Read about d-Drive specifics: `d_drive`
