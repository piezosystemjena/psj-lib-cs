# Developer Guide

This guide is for developers who want to extend psj-lib or add support
for new piezosystem jena devices.

## Architecture Overview

### Library Structure

psj-lib follows a modular, capability-based architecture:

``` text
psj_lib/
├── devices/
│   ├── base/                        # Base classes (Public: PiezoDevice, PiezoChannel, capabilities)
│   │   ├── piezo_device.py         # Abstract device base
│   │   ├── piezo_channel.py        # Abstract channel base
│   │   ├── device_factory.py       # Device registration (internal)
│   │   ├── command_cache.py        # Response caching (internal)
│   │   ├── exceptions.py           # Error hierarchy
│   │   └── capabilities/           # Reusable capability modules (all public)
│   │       ├── position.py
│   │       ├── pid_controller.py
│   │       ├── data_recorder.py
│   │       └── ...
│   │
│   ├── d_drive_family/              # d-Drive family (d-Drive + 30DV series)
│   │   ├── d_drive/                 # d-Drive modular system
│   │   │   ├── d_drive_device.py   # Device class
│   │   │   └── d_drive_channel.py  # Channel class
│   │   ├── psj_30dv/                # 30DV series single-channel device
│   │   │   ├── psj_30dv_device.py  # Device class
│   │   │   └── psj_30dv_channel.py # Channel class
│   │   └── capabilities/           # d-Drive family-specific capabilities
│   │       ├── d_drive_status_register.py
│   │       ├── d_drive_waveform_generator.py
│   │       └── ...
│   │
│   └── transport_protocol/          # Communication layer (internal, except types)
│       ├── transport_protocol.py   # Abstract transport
│       ├── device_discovery.py     # Device discovery
│       ├── serial/                 # Serial implementation
│       └── telnet/                 # Telnet implementation
│
└── _internal/                       # Internal utilities
```

**Note**: Only classes exported in `psj_lib.__init__.py` are part of the
public API. When extending the library, you will work with internal
modules, but end users should only use the public API.

### Design Principles

1.  **Capability-Based**: Features are modular capabilities, not
    monolithic device classes
2.  **Async-First**: All I/O operations use asyncio
3.  **Type-Safe**: Comprehensive type hints for IDE support
4.  **Extensible**: Easy to add new devices and capabilities
5.  **Documented**: Every class and method has comprehensive docstrings

## Adding New Device Types

### Comprehensive Guide

For a complete, step-by-step guide with examples, see:

**File**: `psj_lib/devices/ADDING_NEW_DEVICES.md`

This 1200+ line guide covers:

- Step-by-step device implementation
- Channel discovery patterns
- Capability integration
- Command protocol design
- Status register implementation
- Testing strategies
- Common pitfalls and solutions
- Real-world examples

### Quick Start: New Device

Here's the minimal steps to add a new device type:

**Step 1: Create Device Class**

``` python
# psj_lib/devices/my_device/my_device_device.py

from ..base.piezo_device import PiezoDevice
from .my_device_channel import MyDeviceChannel

class MyDeviceDevice(PiezoDevice):
    """My custom piezo device."""

    # Required: Unique device identifier
    DEVICE_ID = "MyDevice"

    # Required: Maximum supported channels (set to 1 for single-channel devices)
    MAX_CHANNEL_COUNT = 4

    # Optional: Cacheable commands for performance
    CACHEABLE_COMMANDS = {
        "idn?",
        "status?",
        # Add read-only commands that can be cached
    }

    @classmethod
    async def _is_device_type(
        cls,
        transport: TransportProtocol
    ) -> str | None:
        """Check if connected device is this type."""
        try:
            response = await transport.send_command("idn?")
            if "MyDevice" in response:
                return cls.DEVICE_ID
            return None
        except Exception:
            return None

    async def _discover_channels(self) -> list[PiezoChannel]:
        """Discover available channels."""
        # Query number of channels
        num_channels = await self.send_command("numch?")
        num_channels = int(num_channels)

        # Create channel objects
        channels = []
        for i in range(num_channels):
            channel = MyDeviceChannel(self, i)
            channels.append(channel)

        return channels
```

**Step 2: Create Channel Class**

``` python
# psj_lib/devices/my_device/my_device_channel.py

from ..base.piezo_channel import PiezoChannel
from ..base.capabilities import *
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ..base.capabilities import Position, Status

class MyDeviceChannel(PiezoChannel):
    """Single channel of MyDevice."""

    # Add capabilities using CapabilityDescriptor
    status = CapabilityDescriptor(
        Status,
        "Device status register"
    )

    position: Position = CapabilityDescriptor(
        Position,
        "Position control"
    )

    # Add more capabilities as needed...
```

**Step 3: Register Device**

The `DEVICE_ID` class attribute automatically registers the device with
the internal device factory system.

Verify registration (for development/testing):

``` python
from psj_lib.devices.base.device_factory import DeviceFactory

# Should include your device
print(DeviceFactory.get_supported_devices())
```

**Note**: `DeviceFactory` is an internal module. End users will access
your device through `PiezoDevice.discover_devices()` or direct
instantiation (`MyDeviceDevice(...)`), not through the factory.

**Step 4: Test Discovery**

``` python
from psj_lib import PiezoDevice, DiscoverFlags

devices = await PiezoDevice.discover_devices()

# Should find your device if connected
for device in devices:
    print(f"Found: {device.device_id}")
```

## Creating Custom Capabilities

### Base Capability Class

All capabilities inherit from `PiezoCapability`:

``` python
from psj_lib import PiezoCapability

class MyCustomCapability(PiezoCapability):
    """Custom device capability."""

    def __init__(self, channel: PiezoChannel):
        """Initialize capability.

        Args:
            channel: Parent channel this capability belongs to
        """
        super().__init__(channel)
        self._cached_value = None

    async def get_value(self) -> float:
        """Read capability value.

        Returns:
            Current value

        Example:
            >>> value = await capability.get_value()
            >>> print(f"Value: {value:.2f}")
        """
        cmd = f"custom{self.channel.channel_id}:value?"
        response = await self.channel.device.send_command(cmd)
        return float(response)

    async def set_value(self, value: float) -> None:
        """Set capability value.

        Args:
            value: Target value to set

        Example:
            >>> await capability.set_value(50.0)
        """
        cmd = f"custom{self.channel.channel_id}:value {value}"
        await self.channel.device.send_command(cmd)
        self._cached_value = value
```

### Capability Descriptor Pattern

Use `CapabilityDescriptor` for lazy initialization:

``` python
from psj_lib import CapabilityDescriptor

class MyChannel(PiezoChannel):
    """Channel with custom capability."""

    # Capability is created on first access
    my_capability: MyCustomCapability = CapabilityDescriptor(
        MyCustomCapability,
        "My custom feature"
    )

# Usage
channel = MyChannel(device, 0)

# First access creates the capability
value = await channel.my_capability.get_value()

# Subsequent access uses same instance
await channel.my_capability.set_value(100.0)
```

### Status Register Pattern

For devices with bit-mapped status registers, create a custom
`StatusRegister` subclass and use it with the base `Status` capability:

``` python
from psj_lib.devices.base.capabilities import StatusRegister, Status
from psj_lib.devices.base.capabilities import CapabilityDescriptor

class MyDeviceStatusRegister(StatusRegister):
    """Status register interpretation for MyDevice.

    Inherits from StatusRegister base class which holds the raw
    status response in self._raw attribute.
    """

    @property
    def closed_loop_enabled(self) -> bool:
        """Check if closed-loop control is enabled (bit 0).

        Returns:
            True if closed-loop control is active
        """
        val = int(self._raw[0])
        return bool(val & 0x01)

    @property
    def no_error(self) -> bool:
        """Check for error conditions (bit 1).

        Returns:
            True if no errors present
        """
        val = int(self._raw[0])
        return bool(val & 0x02)

    @property
    def data_ready(self) -> bool:
        """Check if data is ready (bit 2).

        Returns:
            True if data is available
        """
        val = int(self._raw[0])
        return bool(val & 0x04)

    # Add more properties for other status bits...

# In your channel class:
class MyDeviceChannel(PiezoChannel):
    """Channel with status register capability."""

    # Use CapabilityDescriptor with register_type parameter
    status_register: Status = CapabilityDescriptor(
        Status,
        {Status.CMD_STATUS: "stat"},
        register_type=MyDeviceStatusRegister
    )

    # Usage example in docstring:
    """
    Example:
        >>> status = await channel.status_register.get()
        >>> if status.closed_loop_enabled:
        ...     print("Closed-loop active")
        >>> if not status.no_error:
        ...     print("Error detected!")
    """
```

## Documentation Standards

### Docstring Format

Use Google-style docstrings with examples:

``` python
async def my_function(param1: float, param2: str) -> bool:
    """One-line summary of function.

    More detailed description of what the function does,
    including any important behavior or side effects.

    Args:
        param1: Description of first parameter
        param2: Description of second parameter

    Returns:
        Description of return value

    Raises:
        ValueError: When param1 is negative
        DeviceError: When device communication fails

    Example:
        >>> result = await my_function(50.0, "test")
        >>> print(result)
        True

    Note:
        Any important notes or warnings about usage.
    """
    pass
```

### Type Hints

Always include type hints:

``` python
from typing import Optional, List, Dict, Any

async def typed_function(
    value: float,
    count: int = 10,
    config: Optional[Dict[str, Any]] = None
) -> List[float]:
    """Function with complete type hints."""
    pass
```

## Contributing Guidelines

### Code Style

Follow PEP 8 and use consistent formatting:

- Use 4 spaces for indentation
- Maximum line length: 100 characters
- Use meaningful variable names
- Add blank lines between logical sections

``` python
# Good
async def calculate_position_error(
    target_position: float,
    actual_position: float
) -> float:
    """Calculate position error."""
    return abs(target_position - actual_position)

# Bad
async def calc(t,a):
    return abs(t-a)
```

### Pull Request Process

1.  **Fork Repository**: Create your own fork
2.  **Create Branch**: Use descriptive branch name
3.  **Implement Changes**: Follow coding standards
4.  **Add Tests**: Include unit tests
5.  **Update Documentation**: Document new features
6.  **Submit PR**: With clear description

### Version Control

Use semantic versioning:

- **Major** (X.0.0): Breaking changes
- **Minor** (0.X.0): New features, backwards compatible
- **Patch** (0.0.X): Bug fixes

## Resources

### Reference Documentation

- **ADDING_NEW_DEVICES.md**: Complete device implementation guide
- **API Reference**: `api`
- **Examples**: `examples`
- **Advanced Topics**: `advanced_topics`

### Community

- **GitHub**: <https://github.com/piezosystemjena/psj-lib>
- **Issues**: Report bugs and request features
- **Discussions**: Ask questions and share ideas

### Getting Help

If you need help implementing a new device:

1.  Read `ADDING_NEW_DEVICES.md` thoroughly
2.  Study existing device implementations (d-Drive)
3.  Check API documentation
4.  Open a GitHub issue with specific questions
5.  Contact piezosystem jena

## Next Steps

- Read complete device guide: `psj_lib/devices/ADDING_NEW_DEVICES.md`
- Study d-Drive implementation as reference
- Review capability patterns: `advanced_topics`
- Explore full API: `api`
