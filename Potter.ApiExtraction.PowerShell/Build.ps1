[CmdletBinding()]
param(
    [Parameter()]
    [string]
    $Configuration = "Debug"
)

Push-Location -Path $PSScriptRoot

[string] $OutDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath(".\bld")
[string] $DependenciesFilePath = "$OutDirectory\dependencies.txt"
[string] $AssemblyLoaderFile = "$PSScriptRoot\AssemblyLoader.cs"
[string] $AssemblyLoaderAssemblyFile = "$OutDirectory\Potter.AssemblyLoader.dll"

# Locate project dependencies.
$projectContent = Get-Content -Path .\Potter.ApiExtraction.PowerShell.pssproj -ErrorAction Stop
$projectDocument = [xml] $projectContent

$projectReferences = $projectDocument.Project.ItemGroup.ProjectReference

# Remove old dependencies.
if (Test-Path -Path $OutDirectory)
{
    Remove-Item -Path "$OutDirectory\*" -Recurse
}

# Create dependency list file.
New-Item -Path $DependenciesFilePath -ItemType File -Force | Out-Null

Add-Type -TypeDefinition (Get-Content -Path $AssemblyLoaderFile -Raw) -OutputAssembly $AssemblyLoaderAssemblyFile

Out-File -FilePath $DependenciesFilePath -Encoding ascii -Append -InputObject "Potter.AssemblyLoader"

# Copy dependency artifacts.
$projectReferences | Where-Object { $_ } | ForEach-Object {

    [string] $name = $_.Name
    [string] $projectFilePath = $_.Include

    Out-File -FilePath $DependenciesFilePath -Encoding ascii -Append -InputObject $name

    Write-Verbose -Message "Copying dependency: $name"
    & dotnet.exe publish $projectFilePath --output $OutDirectory --configuration $Configuration
}

Pop-Location