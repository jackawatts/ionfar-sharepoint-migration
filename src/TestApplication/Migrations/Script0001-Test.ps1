#
# Script0001-Test.ps1
#
<#
	.Synopsis
		Test script
#>
[CmdletBinding(SupportsShouldProcess=$true)]
param(
	$Alpha,
	$Beta,
	$Url
)
$script:ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
#$PSScriptRoot = $MyInvocation.MyCommand.Path | Split-Path
$commonParameters = @{
	"Verbose" = ($PSCmdlet.MyInvocation.BoundParameters["Verbose"] -eq $true);
	"Debug" = ($PSCmdlet.MyInvocation.BoundParameters["Debug"] -eq $true)
}

Write-Host "Running Script0001-Test A:$Alpha B:$Beta"
Write-Verbose "verbose URL: $Url" -Verbose
Write-Warning "warning"

# Errors will stop the script
#Write-Error "error"

Write-Progress "activity" "status"
Write-Output "output"

Write-Output "MyInv: $($MyInvocation.GetType())"
Write-Output "MyCom: $($MyInvocation.MyCommand.GetType())"
Write-Output "PSScriptRoot: $PSScriptRoot"
Write-Output "Test1: $Test1"

# Non-zero exit code will stop the script
#Exit 5
