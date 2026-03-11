# Developer Guide

This guide is for developers who want to extend PsjLib or add support
for new piezosystem jena devices.

## Architecture Overview

### Library Structure

PsjLib follows a modular, capability-based architecture:

``` text
PsjLib/
├── PsjLib.cs
└── Devices/
    ├── Base/
    │   ├── PiezoDevice.cs
    │   ├── PiezoChannel.cs
    │   ├── DeviceFactory.cs
    │   ├── CommandCache.cs
    │   ├── Exceptions.cs
    │   └── Capabilities/
    ├── DDriveFamily/
    │   ├── DDriveFamilyDevice.cs
    │   ├── DDriveFamilyChannel.cs
    │   ├── DDrive/
    │   ├── PSJ30DV/
    │   └── Capabilities/
    ├── NVFamily/
    │   ├── NVFamilyDevice.cs
    │   ├── NVFamilyChannel.cs
    │   ├── NV403/
    │   ├── NV403CLE/
    │   └── Capabilities/
    └── TransportProtocol/
```

### Design Principles

1. **Capability-Based**: Features are composed as focused capability
   classes
2. **Async-First**: Device I/O uses `Task`-based async methods
3. **Type-Safe**: Strongly typed devices, channels, capabilities, and
   enums
4. **Extensible**: New device models can be integrated through derived
   classes and registry wiring
5. **Documented**: XML doc comments and generated API docs

## Adding New Device Types

### Comprehensive Guide

For complete implementation guidance, use existing family/device classes
as references:

- [PiezoDevice](../api/PsjLib.Base.PiezoDevice.yml)
- [DDriveFamilyDevice](../api/PsjLib.DDriveFamily.DDriveFamilyDevice.yml)
- [NVFamilyDevice](../api/PsjLib.NVFamily.NVFamilyDevice.yml)

### Quick Start: New Device

Here's the minimal sequence to add a new model.

**Step 1: Create Device Class**

``` csharp
using PsjLib.Base;
using PsjLib.Transport;

namespace PsjLib.MyFamily;

public sealed class MyDevice(TransportType transportType, string identifier)
    : PiezoDevice(transportType, identifier)
{
    static MyDevice()
    {
        DeviceModelRegistry.Registry["MY-DEVICE-ID"] =
            static (transport, id) => new MyDevice(transport, id);
    }

    public override string? DeviceId => "MY-DEVICE-ID";

    protected override async Task<string?> IsDeviceTypeAsync(TransportProtocol transport)
    {
        // Probe model identifier over transport
        return await Task.FromResult(DeviceId).ConfigureAwait(false);
    }

    protected override Task DiscoverChannelsAsync()
    {
        ChannelsInternal.Clear();
        ChannelsInternal[0] = new MyChannel(0, WriteChannelAsync);
        return Task.CompletedTask;
    }
}
```

**Step 2: Create Channel Class**

``` csharp
using PsjLib.Base;
using PsjLib.Base.Capabilities;

namespace PsjLib.MyFamily;

public sealed class MyChannel : PiezoChannel
{
    public MyChannel(int id, ChannelWriteCallback writeCallback)
        : base(id, writeCallback)
    {
        Position = new Position(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [Position.CmdPosition] = "measure",
        });
    }

    public Position Position { get; }
}
```

**Step 3: Register Device**

Registration occurs in static constructor via
`DeviceModelRegistry.Registry[...] = ...`. The factory can then create
instances from discovered model IDs.

**Step 4: Test Discovery**

``` csharp
using PsjLib.Base;

var devices = await PiezoDevice.DiscoverDevicesAsync<PiezoDevice>().ConfigureAwait(false);
foreach (var device in devices)
{
    Console.WriteLine(device.DeviceInfo);
}
```

## Creating Custom Capabilities

### Base Capability Class

All capabilities derive from [PiezoCapability](../api/PsjLib.Base.Capabilities.PiezoCapability.yml).

``` csharp
using PsjLib.Base.Capabilities;

public sealed class MyCustomCapability(
    CapabilityWriteCallback writeCb,
    IReadOnlyDictionary<string, string> commands)
    : PiezoCapability(writeCb, commands)
{
    public const string CmdValue = "MY_VALUE";

    public async Task<double> GetValueAsync()
        => double.Parse((await WriteAsync(CmdValue).ConfigureAwait(false))[0],
            System.Globalization.CultureInfo.InvariantCulture);

    public async Task SetValueAsync(double value)
        => _ = await WriteAsync(CmdValue, [value]).ConfigureAwait(false);
}
```

### Capability Descriptor Pattern

In this repository, channels instantiate capabilities in their
constructors and expose them as read-only properties.

``` csharp
public sealed class MyChannel : PiezoChannel
{
    public MyChannel(int id, ChannelWriteCallback writeCallback)
        : base(id, writeCallback)
    {
        Custom = new MyCustomCapability(CapabilityWriteAsync, new Dictionary<string, string>
        {
            [MyCustomCapability.CmdValue] = "myvalue",
        });
    }

    public MyCustomCapability Custom { get; }
}
```

### Status Register Pattern

Use `Status<TRegister>` with a strongly typed register parser.

``` csharp
using PsjLib.Base.Capabilities;

public sealed record MyStatusRegister(IReadOnlyList<string> Raw);

public sealed class MyStatusCapability(
    CapabilityWriteCallback writeCb,
    IReadOnlyDictionary<string, string> commands)
    : Status<MyStatusRegister>(writeCb, commands)
{
    protected override MyStatusRegister ParseStatus(IReadOnlyList<string> raw)
        => new(raw);
}
```

## Documentation Standards

### Docstring Format

Use XML doc comments for public APIs.

``` csharp
/// <summary>
/// Moves channel to a target setpoint.
/// </summary>
/// <param name="target">Target value in device units.</param>
/// <returns>Completion task.</returns>
public async Task MoveAsync(double target)
{
    await _channel.Setpoint.SetAsync(target).ConfigureAwait(false);
}
```

### Type Hints

Use explicit C# typing (`Task<T>`, records, enums, generics).

``` csharp
public async Task<IReadOnlyList<double>> ReadSamplesAsync(int count)
{
    // ...
    return new List<double>(count);
}
```

## Contributing Guidelines

### Code Style

Follow existing repository conventions:

- `PascalCase` for public members
- `camelCase` for locals/parameters
- `async` methods end with `Async`
- `ConfigureAwait(false)` in library code
- Keep changes scoped and minimal

### Pull Request Process

1. Fork repository
2. Create feature branch
3. Implement changes + tests where appropriate
4. Update docs
5. Submit PR with clear summary

### Version Control

Use semantic versioning:

- **Major** (`X.0.0`): breaking changes
- **Minor** (`0.X.0`): backward-compatible features
- **Patch** (`0.0.X`): backward-compatible fixes

## Resources

### Reference Documentation

- API Reference: [API Reference](api.md)
- Examples: [Examples](examples.md)
- Device-specific docs: [d-Drive](d_drive.md), [NV Series](nv_series.md)

### Community

- GitHub: <https://github.com/piezosystemjena/psj-lib-cs>
- Issues: bug reports and feature requests

### Getting Help

If you need help implementing support for a new model:

1. Study `DDriveFamily` and `NVFamily` implementations
2. Reuse existing capability classes where possible
3. Add model-specific capabilities only where protocol differs
4. Validate discovery/connect/backup/restore flows

## Next Steps

- Study d-Drive and NV implementations as templates
- Review capability usage in `examples/*.cs`
- Cross-check all exposed public types in [API Reference](api.md)
