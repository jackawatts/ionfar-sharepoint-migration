<#
.SYNOPSIS
	Publishes the IonFar.SharePoint.PowerShell module to PowerShell Gallery

.EXAMPLE
	powershell .\PublishPSModule.ps1 -ApiKey XXX
#>
param(
    [Parameter(Mandatory=$True)]
    [string] $ApiKey,
	[Parameter()]
    [string] $Configuration = "Release"
)

$MODULE_NAME = "IonFar.SharePoint.PowerShell"

# The PS module artefacts (dlls, psd) should be in a directory named with module-name
$SourceDirectory = "$($PSScriptRoot)\IonFar.SharePoint.PowerShell\bin\$($Configuration)"
$ModuleDirectory = "$($SourceDirectory)\$($MODULE_NAME)"
If(!(test-path $ModuleDirectory))
{
      New-Item -ItemType Directory -Path $ModuleDirectory
}
Copy-item -Force -Recurse -Verbose "$($SourceDirectory)\*" -Destination "$($ModuleDirectory)\"


$PublishParams = @{
    NuGetApiKey = $ApiKey
    Path = $ModuleDirectory
    ProjectUri = 'https://github.com/jackawatts/ionfar-sharepoint-migration'
    Tags = @('IonFar', 'SharePoint', 'Deployment')
}

# We install and run PSScriptAnalyzer against the module to make sure it's not failing any tests
Install-Module -Name PSScriptAnalyzer -force
Invoke-ScriptAnalyzer -Path $ModuleDirectory

# ScriptAnalyzer passed! Let's publish
Publish-Module @PublishParams

# The module is now listed on the PowerShell Gallery
Find-Module $MODULE_NAME