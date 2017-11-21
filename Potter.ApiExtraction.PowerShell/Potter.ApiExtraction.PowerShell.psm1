using namespace Potter.ApiExtraction.Core.Configuration
using namespace Potter.ApiExtraction.Core.Generation

function Export-Interfaces
{
    [CmdletBinding()]
    param (
        # Specifies a path to one or more assemblies.
        [Parameter(
            Position = 0,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "Path to one or more assemblies."
        )]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Path,

        # Specifies a path to an API configuration XML file.
        [Parameter(
            Position = 1,
            HelpMessage = "Path to an API configuration XML file."
        )]
        [string]
        $Configuration
    )

    begin
    {
        $typeReader = [ApiTypeReader]::new()

        $typeNameResolver = [TypeNameResolver] @{

            SimplifyNamespaces = $true
        }

        # Read the configuration.
        [ApiConfiguration] $apiConfiguration = $null

        if ($Configuration)
        {
            # Reference: https://stackoverflow.com/q/25991963/2503153 (11/18/2017)
            $configurationFilePath = Resolve-Path -Path $Configuration
            $xmlReaderSettings = [System.Xml.XmlReaderSettings]::new()

            $serializer = [System.Xml.Serialization.XmlSerializer]::new([ApiConfiguration])
            $reader = [System.Xml.XmlReader]::Create($configurationFilePath, $xmlReaderSettings)
            try
            {
                $apiConfiguration = $serializer.Deserialize($reader)
            }
            finally
            {
                $reader.Close()
            }
        }
    }

    process
    {
        if (-not $Path)
        {
            return
        }

        $Path | ForEach-Object {

            $assemblyPath = Resolve-Path -Path $_

            $assemblyLoader = [Potter.Reflection.AssemblyLoader]::new()

            $assembly = $null

            try
            {
                $assembly = $assemblyLoader.Load($assemblyPath)

                if ($assembly)
                {
                    # Force resolution for all types.
                    [void] $assembly.GetTypes()

                    $typeReader.ReadAssembly([System.Reflection.Assembly] $assembly, [TypeNameResolver] $typeNameResolver, [ApiConfiguration] $apiConfiguration)
                }
            }
            finally
            {
                $assemblyLoader.Dispose()
            }
        }
    }
}

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

            $results = [Potter.ApiExtraction.Core.Utilities.ApiTypeReaderExtensions]::ReadResults($apiTypeReader, $configurationPath)

            $assemblyLoader = [Potter.Reflection.AssemblyLoader]::new()

            try
            {
                $results | ForEach-Object {

                    Write-Verbose "$($_.Namespace).$($_.Name)"
                    return [Potter.ApiExtraction.Core.Utilities.ApiExtractionResult] $_
                }
            }
            finally
            {
                $assemblyLoader.Dispose()
            }
        }
    }
}

function Write-CompilationUnit
{
    [CmdletBinding()]
    param (
        # Specifies a path to an API configuration XML file.
        [Parameter(
            Mandatory = $true,
            Position = 0,
            HelpMessage = "Path to an API configuration XML file."
        )]
        [string]
        $Destination,

        # Specifies one or more compilation units to write.
        [Parameter(
            Mandatory = $true,
            Position = 1,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "Compilation units to write."
        )]
        [ValidateNotNullOrEmpty()]
        [Alias("Unit")]
        [string]
        $CompilationUnit,

        # Specifies the names for each compilation unit.
        [Parameter(
            Mandatory = $true,
            Position = 2,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The names for each compilation unit."
        )]
        [string]
        $Name,

        # Specifies the namespaces for each compilation unit.
        [Parameter(
            Position = 3,
            ValueFromPipelineByPropertyName = $true,
            HelpMessage = "The namespaces for each compilation unit."
        )]
        [string]
        $Namespace
    )

    process
    {
        if ($CompilationUnit)
        {
            if (-not (Test-Path -Path $Destination))
            {
                New-Item -Path $Destination -ItemType Directory | Out-Null
            }

            Set-Content -Value $CompilationUnit -Path (Join-Path -Path $Destination -ChildPath "$Name.cs")
        }
    }
}
