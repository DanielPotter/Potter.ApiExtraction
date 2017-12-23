Write-Verbose "Build module" -Verbose
. "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell\Build.ps1"

Write-Verbose "Import module" -Verbose
Import-Module "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell" -Verbose

Write-Verbose "Read assembly" -Verbose
$files = Read-AssemblyApi -Path "$PSScriptRoot\UniversalApiContract.xml" -Verbose

Write-Verbose "Write types" -Verbose
$files | Write-SourceFile -Destination "$PSScriptRoot\UnifiedDeviceApi" -Verbose

$global:LastError = $null
if ($Error)
{
    $global:LastError = $Error[0]

    $LastError | Format-List -Force

    $LastError.Exception | Format-List -Force

    if ($LastError.Exception.InnerException)
    {
        $LastError.Exception.InnerException | Format-List -Force
    }
}
