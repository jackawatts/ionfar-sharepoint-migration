<#
.SYNOPSIS
	Runs file synchronisation using IonFar PowerShell module

.EXAMPLE
	powershell .\RunFileSync.ps1 -SiteUrl https://tenant.sharepoint.com/sites/site-name -UserName XXX -Password XXX
#>
param(
    [Parameter(Mandatory=$True)]
    [string] $SiteUrl,

    [Parameter(Mandatory=$True)]
    [string] $UserName,

    [Parameter(Mandatory=$True)]
    [string] $Password
)

Install-Module "IonFar.SharePoint.PowerShell" -Scope CurrentUser -Force

Import-Module "IonFar.SharePoint.PowerShell"

Invoke-IonFarFileSync -SiteUrl $SiteUrl -UserName $UserName -Password $Password `
			-BaseDirectory "$($PSScriptRoot)" `
			-SourcePath "Files\Style Library" `
			-DestinationPath "~site/Style Library"