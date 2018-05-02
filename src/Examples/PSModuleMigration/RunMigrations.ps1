<#
.SYNOPSIS
	Runs script migrations using IonFar PowerShell module

.EXAMPLE
	powershell .\RunMigrations.ps1 -SiteUrl https://tenant.sharepoint.com/sites/site-name -UserName XXX -Password XXX
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

Invoke-IonFarScriptMigration -SiteUrl $SiteUrl -UserName $UserName -Password $Password -ScriptDirectory "$($PSScriptRoot)\Migrations"