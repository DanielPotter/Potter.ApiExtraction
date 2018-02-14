# API Extractor

A tool for generating interfaces from an assembly.

## Usage

The easiest way to use this tool is via the PowerShell module in this repository.

```XML
<Api xmlns="http://schemas.danielrpotter.com/api/configuration/2017">
  <Assemblies>
    <Assembly Name="Windows.Foundation, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime" Load="false" />
    <Assembly Name="Windows.Foundation.FoundationContract, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime" />
    <Assembly Name="Windows.Foundation.UniversalApiContract, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime" />
  </Assemblies>
  <Names>
    <Namespace Name="Windows.Devices" NewName="Potter.Unified.Devices" Recursive="true" />
    <Namespace Name="Windows.Foundation" NewName="Potter.Unified.Foundation" Recursive="true" />
  </Names>
  <Groups>
    <Group Name="Potter.Unified.Devices.Geolocation">
      <Types Mode="Whitelist">
        <Namespace Name="Windows.Devices.Geolocation" Recursive="true" />
      </Types>
    </Group>
    <Group Name="Potter.Unified.Foundation">
      <Types Mode="Whitelist">
        <Type Name="Windows.Foundation.TypedEventHandler`2" />
      </Types>
    </Group>
  </Groups>
</Api>
```

```PowerShell
$repository = "."
$configuration = ".\ApiConfiguration.xml"
$output = ".\bin"

# Build Module
. "$repository\Potter.ApiExtraction.PowerShell\Build.ps1"

# Import Module
Import-Module "$repository\Potter.ApiExtraction.PowerShell"

# Read Assembly
$files = Read-AssemblyApi -Path $configuration

# Write Interfaces
$files | Write-SourceFile -Destination $output
```

## Planned

* [ ] Create issues for these items
* [ ] Publish PowerShell module to the PowerShell gallery
* [X] Allow transformed interfaces and other types to use a name provided in the configuration.
* [ ] Allow partial configurations
  * E.g. Separate files for type name transformations and type filtering.
  * This would make splitting up an assembly into multiple libraries much easier.
* [X] Support type unions
* [X] Generate a nested folder structure by namespace
* [ ] Support member filters
* [ ] Support explicit type transformations
* [ ] Format generated code
* [ ] Find a way to reduce redundancy in the XML configuration
* [ ] Provide documentation from XML documentation
* [ ] Provide warnings for unhandled types
  * Types that are from a non-standard (or unspecified) assembly and are not transformed should result in a warning.
