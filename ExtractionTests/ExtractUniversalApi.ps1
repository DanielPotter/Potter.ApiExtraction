Write-Verbose "Building module" -Verbose
. "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell\Build.ps1"

Write-Verbose "Importing module" -Verbose
Import-Module "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell" -Verbose

Write-Verbose "Reading assembly" -Verbose
$files = Read-AssemblyApi "$PSScriptRoot\UniversalApiContract.xml" -Verbose

Write-Verbose "Write interfaces" -Verbose
$files | Write-CompilationUnit -Destination "$PSScriptRoot\bin" -Verbose

if ($Error)
{
    $Error[0].Exception | Format-List -Force
}

Pause
Exit-PSHostProcess
