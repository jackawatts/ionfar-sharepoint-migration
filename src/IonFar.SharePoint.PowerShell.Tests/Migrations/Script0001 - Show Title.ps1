[CmdletBinding(SupportsShouldProcess=$true)]
param(
	$Url,
	$Credentials
	)
Set-StrictMode -Version Latest
$script:ErrorActionPreference = 'Stop'
$commonParameters = @{
	"Verbose" = ($PSCmdlet.MyInvocation.BoundParameters["Verbose"] -eq $true);
	"Debug" = ($PSCmdlet.MyInvocation.BoundParameters["Debug"] -eq $true)
}

Write-Output "Running migration for URL: $Url"
Write-Host "Script folder: $PSScriptRoot"

$web = $SPContext.Web

$SPContext.Load($web)
$SPContext.ExecuteQuery()

Write-Warning "Site title is: $($web.Title)"
