REM .\RunAllExamples.cmd https://contoso.sharepoint.com/sites/test-ionfar user@contoso.onmicrosoft.com password
set url=%1
set username=%2
set password=%3
.\Examples\BasicCodeMigration\bin\Debug\BasicCodeMigration.exe %url% %username% %password%
.\Examples\BasicScriptMigration\bin\Debug\BasicScriptMigration.exe %url% %username% %password%
.\Examples\BasicSynchronization\bin\Debug\BasicSynchronization.exe %url% %username% %password%
.\Examples\TestApplication\bin\Debug\TestApplication.exe %url% %username% %password%
