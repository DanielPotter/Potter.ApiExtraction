# API Extractor

A tool for generating interfaces from an assembly.

## Usage

The easiest way to use this tool is via the PowerShell module in this repository.

```XML
<Api xmlns="http://schemas.danielrpotter.com/api/configuration/2017">
    <Assembly Name="Windows.Foundation.UniversalApiContract, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime" />
    <Types Mode="Whitelist" SimplifyTypeNames="true">
        <Namespace Name="Windows.Devices.Geolocation" />
    </Types>
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

- Publish PowerShell module to the PowerShell gallery.
- Allow transformed interfaces and other types to use a name provided in the configuration.
- Allow partial configurations.
  - E.g. Separate files for type name transformations and type filtering.
  - This would make splitting up an assembly into multiple libraries much easier.
- Provide warnings for unhandled types.
  - Types that are from a non-standard (or unspecified) assembly and are not transformed should result in a warning.
