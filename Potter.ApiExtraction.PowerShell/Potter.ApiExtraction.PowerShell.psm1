using namespace Potter.ApiExtraction.Core.Configuration
using namespace Potter.ApiExtraction.Core.Generation
using namespace Potter.ApiExtraction.Core.Utilities

function Read-AssemblyApi
{
    [CmdletBinding()]
    param (
        # Specifies a path to one or more configuration files.
        [Parameter(
            Position = 0,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "Path to one or more configuration files."
        )]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Path
    )

    process
    {
        $Path | ForEach-Object {

            [string] $configurationPath = $_

            $apiTypeReader = [ApiTypeReader]::new()

            $results = [ApiTypeReaderExtensions]::ReadResults($apiTypeReader, $configurationPath)

            $assemblyLoader = [Potter.Reflection.AssemblyLoader]::new()

            try
            {
                $results | ForEach-Object {

                    Write-Verbose "$($_.Group)\$($_.Namespace).$($_.Name)"
                    return [ApiExtractionResult] $_
                }
            }
            finally
            {
                $assemblyLoader.Dispose()
            }
        }
    }
}

function Write-SourceFile
{
    [CmdletBinding()]
    param (
        # Specifies a path to a directory into which the source file should be written.
        [Parameter(
            Mandatory = $true,
            Position = 0,
            HelpMessage = "The directory into which the source file should be written."
        )]
        [string]
        $Destination,

        # Specifies the source code text to write.
        [Parameter(
            Mandatory = $true,
            Position = 1,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The source code to write."
        )]
        [ValidateNotNullOrEmpty()]
        [Alias("Source")]
        [string]
        $SourceCode,

        # Specifies the name of the code unit (e.g. the type name) that will be used for the name of the file.
        [Parameter(
            Mandatory = $true,
            Position = 2,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The name of the code unit (e.g. the type name)."
        )]
        [string]
        $Name,

        # Specifies the sub-folder into which the source file should be written.
        [Parameter(
            Position = 3,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The sub-folder into which the source file should be written."
        )]
        [string]
        $Group
    )

    process
    {
        if ($SourceCode)
        {
            if (-not (Test-Path -Path $Destination))
            {
                New-Item -Path $Destination -ItemType Directory | Out-Null
            }

            $destinationPath = $Destination

            if ($Group)
            {
                $destinationPath = Join-Path -Path $destinationPath -ChildPath $Group
            }

            $destinationPath = Join-Path -Path $destinationPath -ChildPath "$Name.cs"

            Write-Verbose -Message "Writing file: $destinationPath"
            Set-Content -Value $SourceCode -Path $destinationPath
        }
    }
}
