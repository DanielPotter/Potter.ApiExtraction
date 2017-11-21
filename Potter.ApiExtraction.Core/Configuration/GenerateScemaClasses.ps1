$XsdFilterPath = 'C:\Program Files (x86)\Microsoft SDKs\Windows\*\bin\NETFX*Tools\xsd.exe'
$TargetFile = "$PSScriptRoot\ApiConfiguration.xsd"
$OutputDirectoryPath = $PSScriptRoot
$OuputNamespace = 'Potter.ApiExtraction.Core.Configuration'

# Validate parameters.
if (-not (Test-Path $TargetFile))
{
    throw "Could not locate target file: $TargetFile"
}

# The last result should have the greatest version.
$xsdFile = Get-ChildItem -Path $XsdFilterPath | Select-Object -Last 1

if (-not $xsdFile)
{
    throw "Could not locate xsd.exe"
}

& $xsdFile.FullName $TargetFile /classes /language:CS /namespace:$OuputNamespace /nologo /out:$OutputDirectoryPath
