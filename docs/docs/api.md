# API Reference

Complete API reference overview for psj-lib.

## Quick Navigation

**Core Components**

- [Devices](#devices) - Device and channel classes
- [Base Capabilities](#base-capabilities) - Common capabilities across
  all devices
- [NV Family](#nv-family) - NV-series device and capability classes
- [d-Drive Specific Capabilities](#d-drive-specific-capabilities) -
  d-Drive enhanced features
- [Exceptions](#exceptions) - Error handling
- [Type Definitions](#type-definitions) - Enums and records

**Base Capabilities by Category**

- [Status and Monitoring](#status-and-monitoring)
- [Position Control](#position-control)
- [Control System](#control-system)
- [Filters](#filters)
- [Signal Generation](#signal-generation)
- [Data Acquisition](#data-acquisition)
- [Configuration](#configuration)

**Usage Patterns**

- See [Examples](examples.md) for practical usage patterns.

## Package Structure

``` text
PsjLib/
├── PsjLib.cs
└── Devices/
    ├── Base/
    │   ├── PiezoDevice.cs
    │   ├── PiezoChannel.cs
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
    │   ├── NV120/
    │   ├── NV120CLE/
    │   ├── NV403/
    │   ├── NV403CLE/
    │   └── Capabilities/
    └── TransportProtocol/
```

## Core Modules

### Devices

- [PiezoDevice](../api/PsjLib.Base.PiezoDevice.yml)
- [PiezoChannel](../api/PsjLib.Base.PiezoChannel.yml)

### Base Device Classes

- [PiezoDevice](../api/PsjLib.Base.PiezoDevice.yml)
- [PiezoChannel](../api/PsjLib.Base.PiezoChannel.yml)

### d-Drive Family

- [DDriveFamilyDevice](../api/PsjLib.DDriveFamily.DDriveFamilyDevice.yml)
- [DDriveFamilyChannel](../api/PsjLib.DDriveFamily.DDriveFamilyChannel.yml)
- [DDriveDevice](../api/PsjLib.DDriveFamily.DDriveDevice.yml)
- [DDriveChannel](../api/PsjLib.DDriveFamily.DDriveChannel.yml)
- [PSJ30DVDevice](../api/PsjLib.DDriveFamily.PSJ30DVDevice.yml)
- [PSJ30DVChannel](../api/PsjLib.DDriveFamily.PSJ30DVChannel.yml)

### NV Family

- [NVFamilyDevice](../api/PsjLib.NVFamily.NVFamilyDevice.yml)
- [NVFamilyChannel](../api/PsjLib.NVFamily.NVFamilyChannel.yml)
- [NV120Device](../api/PsjLib.NVFamily.NV120.NV120Device.yml)
- [NV120Channel](../api/PsjLib.NVFamily.NV120.NV120Channel.yml)
- [NV120CLEDevice](../api/PsjLib.NVFamily.NV120CLE.NV120CLEDevice.yml)
- [NV120CLEChannel](../api/PsjLib.NVFamily.NV120CLE.NV120CLEChannel.yml)
- [NV403Device](../api/PsjLib.NVFamily.NV403.NV403Device.yml)
- [NV403Channel](../api/PsjLib.NVFamily.NV403.NV403Channel.yml)
- [NV403CLEDevice](../api/PsjLib.NVFamily.NV403CLE.NV403CLEDevice.yml)
- [NV403CLEChannel](../api/PsjLib.NVFamily.NV403CLE.NV403CLEChannel.yml)

### Base Capabilities

#### Status and Monitoring

- [Status<TRegister>](../api/PsjLib.Base.Capabilities.Status-1.yml)
- [Temperature](../api/PsjLib.Base.Capabilities.Temperature.yml)
- [ActuatorDescription](../api/PsjLib.Base.Capabilities.ActuatorDescription.yml)

#### Position Control

- [Position](../api/PsjLib.Base.Capabilities.Position.yml)
- [Setpoint](../api/PsjLib.Base.Capabilities.Setpoint.yml)
- [ClosedLoopController](../api/PsjLib.Base.Capabilities.ClosedLoopController.yml)
- [SlewRate](../api/PsjLib.Base.Capabilities.SlewRate.yml)

#### Control System

- [PIDController](../api/PsjLib.Base.Capabilities.PIDController.yml)
- [PreControlFactor](../api/PsjLib.Base.Capabilities.PreControlFactor.yml)

#### Filters

- [NotchFilter](../api/PsjLib.Base.Capabilities.NotchFilter.yml)
- [LowPassFilter](../api/PsjLib.Base.Capabilities.LowPassFilter.yml)
- [ErrorLowPassFilter](../api/PsjLib.Base.Capabilities.ErrorLowPassFilter.yml)

#### Signal Generation

- [ModulationSource](../api/PsjLib.Base.Capabilities.ModulationSource.yml)
- [MonitorOutput](../api/PsjLib.Base.Capabilities.MonitorOutput.yml)
- [StaticWaveformGenerator](../api/PsjLib.Base.Capabilities.StaticWaveformGenerator.yml)

#### Data Acquisition

- [DataRecorder](../api/PsjLib.Base.Capabilities.DataRecorder.yml)
- [TriggerOut](../api/PsjLib.Base.Capabilities.TriggerOut.yml)

#### Configuration

- [Unit](../api/PsjLib.Base.Capabilities.Unit.yml)
- [Limits](../api/PsjLib.Base.Capabilities.Limits.yml)
- [Display](../api/PsjLib.Base.Capabilities.Display.yml)
- [MultiSetpoint](../api/PsjLib.Base.Capabilities.MultiSetpoint.yml)
- [MultiPosition](../api/PsjLib.Base.Capabilities.MultiPosition.yml)
- [FactoryReset](../api/PsjLib.Base.Capabilities.FactoryReset.yml)
- [Fan](../api/PsjLib.Base.Capabilities.Fan.yml)

### d-Drive Specific Capabilities

- [DDriveStatusRegister](../api/PsjLib.DDriveFamily.Capabilities.DDriveStatusRegister.yml)
- [DDriveWaveformGenerator](../api/PsjLib.DDriveFamily.Capabilities.DDriveWaveformGenerator.yml)
- [DDriveDataRecorder](../api/PsjLib.DDriveFamily.Capabilities.DDriveDataRecorder.yml)
- [DDriveTriggerOut](../api/PsjLib.DDriveFamily.Capabilities.DDriveTriggerOut.yml)
- [DDriveModulationSourceTypes](../api/PsjLib.DDriveFamily.Capabilities.DDriveModulationSourceTypes.yml)
- [DDriveMonitorOutputSource](../api/PsjLib.DDriveFamily.Capabilities.DDriveMonitorOutputSource.yml)

### NV Specific Capabilities

- [NVDisplay](../api/PsjLib.NVFamily.Capabilities.NVDisplay.yml)
- [NVKnob](../api/PsjLib.NVFamily.Capabilities.NVKnob.yml)
- [NVCLEKnob](../api/PsjLib.NVFamily.Capabilities.NVCLEKnob.yml)
- [NVModulationSource](../api/PsjLib.NVFamily.Capabilities.NVModulationSource.yml)
- [NVMonitorOutput](../api/PsjLib.NVFamily.Capabilities.NVMonitorOutput.yml)
- [NVSetpoint](../api/PsjLib.NVFamily.Capabilities.NVSetpoint.yml)
- [NVStatusRegister](../api/PsjLib.NVFamily.Capabilities.NVStatusRegister.yml)

### Exceptions

- [DeviceError](../api/PsjLib.Base.DeviceError.yml)
- [UnknownCommand](../api/PsjLib.Base.UnknownCommand.yml)
- [ParameterMissing](../api/PsjLib.Base.ParameterMissing.yml)
- [AdmissibleParameterRangeExceeded](../api/PsjLib.Base.AdmissibleParameterRangeExceeded.yml)
- [CommandParameterCountExceeded](../api/PsjLib.Base.CommandParameterCountExceeded.yml)
- [ParameterLockedOrReadOnly](../api/PsjLib.Base.ParameterLockedOrReadOnly.yml)
- [Underload](../api/PsjLib.Base.Underload.yml)
- [Overload](../api/PsjLib.Base.Overload.yml)
- [ParameterTooLow](../api/PsjLib.Base.ParameterTooLow.yml)
- [ParameterTooHigh](../api/PsjLib.Base.ParameterTooHigh.yml)
- [UnknownChannel](../api/PsjLib.Base.UnknownChannel.yml)
- [ActuatorNotConnected](../api/PsjLib.Base.ActuatorNotConnected.yml)
- [ProtocolException](../api/PsjLib.Transport.ProtocolException.yml)
- [DeviceUnavailableException](../api/PsjLib.Transport.DeviceUnavailableException.yml)
- [TimeoutException](../api/PsjLib.Transport.TimeoutException.yml)

### Type Definitions

- [TransportType](../api/PsjLib.Transport.TransportType.yml)
- [DiscoverFlags](../api/PsjLib.Transport.DiscoverFlags.yml)
- [TransportProtocolInfo](../api/PsjLib.Transport.TransportProtocolInfo.yml)
- [DeviceInfo](../api/PsjLib.Base.DeviceInfo.yml)
