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

        if (-not $apiConfiguration)
        {
            $apiConfiguration = [ApiConfiguration]::new()
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

            [System.Reflection.Assembly] $assembly = [Potter.Reflection.AssemblyLoader]::Load($assemblyPath)

            $assembly.GetTypes()
            #$typeReader.ReadAssembly($assembly, $apiConfiguration, $typeNameResolver)
        }
    }
}
