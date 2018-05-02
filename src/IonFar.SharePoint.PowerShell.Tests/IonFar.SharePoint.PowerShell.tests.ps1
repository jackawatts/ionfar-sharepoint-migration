Import-Module "$($PSScriptRoot)\..\IonFar.SharePoint.PowerShell\bin\Debug\IonFar.SharePoint.PowerShell.psd1"

Describe "Import-Module" {

	Context "Get-Command Invoke-IonFarScriptMigration" {		
		It "Invoke-IonFarScriptMigration Cmdlet Available" {
			$cmd = Get-Command Invoke-IonFarScriptMigration
			$cmd | Should Not Be $null
		}
	}

	Context "Get-Command Invoke-IonFarFileSync" {		
		It "Invoke-IonFarFileSync Cmdlet Available" {
			$cmd = Get-Command Invoke-IonFarFileSync
			$cmd | Should Not Be $null
		}
	}

	Context "Invoke-IonFarScriptMigration -Force" {
		It "Invoke-IonFarScriptMigration runs local with -Force flag" {
			$result = Invoke-IonFarScriptMigration -SiteUrl "https://dummy.sharepoint.com" `
				-UserName "dummyuser@dummy.onmicrosoft.com" `
				-Password "dummypwd" `
				-ScriptDirectory "$($PSScriptRoot)\Migrations" `
				-Force

			$result.Successful | Should Be $true
		}
	}
}
