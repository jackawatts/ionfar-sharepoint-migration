REM Creates a NuGet package
if not exist Output mkdir Output
.nuget\nuget.exe pack IonFar.SharePoint.Migration\IonFar.SharePoint.Migration.csproj -Build -OutputDirectory Output
