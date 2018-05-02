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

Import-Module "$($PSScriptRoot)\..\..\IonFar.SharePoint.PowerShell\bin\Debug\IonFar.SharePoint.PowerShell.psd1"

Get-Help Invoke-ScriptMigration

Invoke-ScriptMigration -SiteUrl $SiteUrl -UserName $UserName -Password $Password -ScriptDirectory "$($PSScriptRoot)\Migrations"