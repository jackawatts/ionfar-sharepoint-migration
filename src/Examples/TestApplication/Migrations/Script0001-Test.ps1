#
# Script0001-Test.ps1
#
<#
	.Synopsis
		Test script
#>
[CmdletBinding(SupportsShouldProcess=$true)]
param(
	$Url,
	$Credentials,
	$Other
)
# Throws errors if variables not defined
Set-StrictMode -Version Latest
# This is set by default anyway, but good if script is run separately
$script:ErrorActionPreference = 'Stop'
# This works, but is no longer needed since PSH 3
#$PSScriptRoot = $MyInvocation.MyCommand.Path | Split-Path
$commonParameters = @{
	"Verbose" = ($PSCmdlet.MyInvocation.BoundParameters["Verbose"] -eq $true);
	"Debug" = ($PSCmdlet.MyInvocation.BoundParameters["Debug"] -eq $true)
}

Write-Host "Running Script0001-Test"

Write-Output "PSScriptRoot: $PSScriptRoot"
Write-Output "Parameters:"
Write-Output "  Url: $Url"
Write-Output "  Credentials: $Credentials"
Write-Output "  Other: $Other"

# Need to be defined if script is run separately
Write-Output "Variables:"
Write-Output "  SPContext: $SPContext"
Write-Output "  SPUrl: $SPUrl"
Write-Output "  SPCredentials: $SPCredentials"
Write-Output "  SPUserName: $SPUserName"
Write-Output "  SPSecurePassword: $SPSecurePassword"
Write-Output "  SPPassword: $SPPassword"
Write-Output "  SPVariables: $SPVariables [$($SPVariables['Other'])]"

Write-Host "A host message"
Write-Output "Output string"
Write-Output ([DateTimeOffset]::Now)
Write-Verbose "A verbose message" -Verbose
Write-Warning "A warning message"
Write-Progress "activity" "status"

# Using .NET directly -- from variable (could also use parameter)
$web = $SPContext.Web
$SPContext.Load($web)
$SPContext.ExecuteQuery()
Write-Output "Web (.NET): $($web.Title)"

# Using PowerShell Cmdlets -- from parameters (could also use variable)
#Import-Module OfficeDevPnP.PowerShell.Commands
Connect-SPOnline –Url $Url -Credentials $Credentials
$web = Get-SPOWeb
Write-Output "Web (Cmdlet): $($web.Title)"

Write-Host "Script Done"

# Errors will stop the script
#Write-Error "An error"

# Throw error also stops script
#Throw "Thrown error"
