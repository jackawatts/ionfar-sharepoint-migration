Import-Module "$($PSScriptRoot)\..\IonFar.SharePoint.PowerShell\bin\Debug\IonFar.SharePoint.PowerShell.psd1"

Describe "Import-Module" {

	Context "Get-Command Invoke-ScriptMigration" {		
		It "Invoke-ScriptMigration Cmdlet Available" {
			$cmd = Get-Command Invoke-ScriptMigration
			$cmd | Should Not Be $null
		}
	}

	Context "Get-Command Invoke-FileSync" {		
		It "Invoke-FileSync Cmdlet Available" {
			$cmd = Get-Command Invoke-FileSync
			$cmd | Should Not Be $null
		}
	}

	Context "Invoke-ScriptMigration -Force" {
		It "Invoke-ScriptMigration runs local with -Force flag" {
			$result = Invoke-ScriptMigration -SiteUrl "https://dummy.sharepoint.com" `
				-UserName "dummyuser@dummy.onmicrosoft.com" `
				-Password "dummypwd" `
				-ScriptDirectory "$($PSScriptRoot)\Migrations" `
				-Force

			$result.Successful | Should Be $true
		}
	}
}
