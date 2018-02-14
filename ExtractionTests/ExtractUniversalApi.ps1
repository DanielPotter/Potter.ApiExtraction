try
{
    $global:LastError = $null

    Write-Verbose "Build module" -Verbose
    . "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell\Build.ps1"

    Write-Verbose "Import module" -Verbose
    Import-Module "$PSScriptRoot\..\Potter.ApiExtraction.PowerShell" -Verbose

    Write-Verbose "Clean output" -Verbose
    Remove-Item -Path "$PSScriptRoot\UnifiedDeviceApi\*\*.cs" -Recurse -Force

    Write-Verbose "Read assembly" -Verbose
    $files = Read-AssemblyApi -Path "$PSScriptRoot\UniversalApiContract.xml" -Verbose

    Write-Verbose "Write types" -Verbose
    $files | Write-SourceFile -Destination "$PSScriptRoot\UnifiedDeviceApi" -Verbose
}
catch
{
    if ($Error)
    {
        Write-Verbose "One or more errors ocurred." -Verbose

        $global:LastError = $Error[0]

        # Test the exception target.
        $hasValidTarget = $false
        try
        {
            $target = $LastError.TargetObject | Out-String
            $hasValidTarget = $true
        }
        catch
        {
            Write-Verbose "Invalid exception target." -Verbose
        }

        $properties = $LastError |
            Get-Member -MemberType Properties |
            Foreach-Object Name |
            Where-Object { $_ -ne 'Exception' }

        Write-Verbose "LastError:" -Verbose
        if ($hasValidTarget)
        {
            $LastError | Format-List -Force -Property $properties
        }
        else
        {
            $properties = $properties | Where-Object { $_ -ne 'TargetObject' }
            $LastError | Format-List -Force -Property $properties
        }

        $properties = $LastError.Exception |
            Get-Member -MemberType Properties |
            Foreach-Object Name |
            Where-Object { $_ -ne 'InnerException' }

        Write-Verbose "LastError.Exception:" -Verbose
        $LastError.Exception | Format-List -Force -Property $properties

        if ($LastError.Exception.InnerException)
        {
            Write-Verbose "LastError.Exception.InnerException:" -Verbose
            $LastError.Exception.InnerException | Format-List -Force
        }
    }
}
