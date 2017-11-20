# Import dependencies.
Get-Content -Path "$PSScriptRoot\bld\dependencies.txt" |
    Where-Object { $_ } |
    ForEach-Object { "$PSScriptRoot\bld\$_.dll" } |
    Get-Item |
    ForEach-Object {

    Write-Verbose "Loading assembly: $($_.FullName)"
    [void] [System.Reflection.Assembly]::LoadFrom($_)
}
