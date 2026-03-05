# psj-lib: Piezosystem Jena Device Library

![image](images/psj-lib-header.png)

**psj-lib** is a comprehensive Python library for controlling
piezoelectric amplifiers and control devices manufactured by
[piezosystem jena GmbH](https://www.piezosystem.com). The library
provides an intuitive, asynchronous interface for precision position
control, waveform generation, data acquisition, and advanced control
system configuration.

**Key Features:**

- **Asynchronous Architecture**: Built on Python's asyncio for
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

**Currently Supported Devices:**

- **d-Drive**: Modular piezo amplifiers with 20-bit resolution, 50 kHz
  sampling, and advanced control features
- **30DV50/300**: Single-channel, standalone amplifier
- **NV120/1, NV120CLE, NV40/3, NV40/3CLE**: NV-series amplifiers with
  open-loop and closed-loop variants

**Not currently supported:**

- **NV200D/NET**: Please use
  [nv200-python-lib](https://github.com/piezosystemjena/nv200-python-lib)
  for NV200D/NET devices

**Quick Start:**

``` python
import asyncio
from psj_lib import DDriveDevice, TransportType

async def main():
    device = DDriveDevice(TransportType.SERIAL, "COM3")

    async with device:
        channel = device.channels[0]
        await channel.closed_loop_controller.set(True)
        await channel.setpoint.set(50.0)
        print(f"Position: {await channel.position.get():.2f} µm")

asyncio.run(main())
```

## Documentation

<div class="toctree" maxdepth="2" caption="Getting Started:">

intro installation connecting getting_started

</div>

<div class="toctree" maxdepth="2" caption="Device Documentation:">

d_drive nv_series base_capabilities

</div>

<div class="toctree" maxdepth="2" caption="Reference:">

api examples developer_guide

</div>

## Indices and Tables

- `genindex`
- `modindex`
- `search`
