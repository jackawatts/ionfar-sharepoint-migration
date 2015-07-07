# ionfar-sharepoint-migration

ionfar-sharepoint-migration is a .NET library that helps you to deploy and manage changes to 
a SharePoint Site collection using the client side object model (CSOM).  Changes are recorded 
once run in a SharePoint environment so that only changes that need to be run will be applied.

The design is based on DbUp, a similar migration tool for SQL databases, and works by applying
successive migrations against the target SharePoint instance. Both code-based and PowerShell
script-based migrations are supported.

The library also includes a synchronizer component that synchronizes a local folder of files
with a target SharePoint server. It keeps track of the hash values of files uploaded, and
only replaces a file when it has changed. The component takes care of all the check out,
check in and publishing required.

Using a console app to automate the deployment of your SharePoint development allows it to 
be easily integrated with automated build and deployment tools.


## Basic code migration

Create a console application and reference the library.

In the main function create a configuration with an AssemblyMigrationProvider, then
create and run an Migrator. Initially this will do nothing (as it has no migrations
to perform).

```
var config = new MigratorConfiguration();       
config.Log = new ConsoleUpgradeLog();
config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetExecutingAssembly()));
config.ContextManager = new BasicContextManager(webUrl, username, password);

var migrator = new Migrator(config);
var result = migrator.PerformMigration();
```

A migration can be created by implementing IMigration (or inheriting from Migration) and 
adding the [Migration("")] attribute. Migrations are run in order based on the name specified
in the attribute; a numerical counter works well, e.g. "Migration0001".

```
[Migration("Code0001")]
public class ShowTitle : Migration
{
    public override void Apply(IContextManager contextManager, IUpgradeLog logger)
    {
        logger.Information("Running migration for URL: {0}", contextManager.CurrentContext.Url);
		// Code to perform migration
    }
}
```

The minimal migration above will simply print out a message.

NOTE: The migration will only be run ONCE in an environment, and then store the name in
a web property bag. The second time you run the script nothing will happen.

You can get the migration to run every time by using NullJournal().


## Basic script migration

The engine also supports PowerShell script-based migrations. To make best use of this the
SharePoint Online management tools should be installed; even better with an extension
library like the Office Dev PnP PowerShell extensions.

The basic configuration is similar:

```
var config = new MigratorConfiguration();       
config.Log = new ConsoleUpgradeLog();
config.MigrationProviders.Add(new ScriptMigrationProvider(scriptsSource));
config.ContextManager = new BasicContextManager(webUrl, username, password);

var migrator = new Migrator(config);
var result = migrator.PerformMigration();
```

The PowerShell scripts need to be stored in the folder scriptsSource (mark then as 'Copy if newer',
if you want them in the project output directory).

```
param ($Url, $Credentials)
Write-Output "Running migration for URL: $SPUrl"
Write-Host "Script folder: $PSScriptRoot"
```

The engine sets PowerShell variables for: $SPContext, $SPUrl, $SPCredentials, $SPUserName, 
$SPSecurePassword, $SPPassword, and $SPVariables (custom variables).

It will also pass in parameters for: $Context, $Url, $Credentials, $UserName, $SecurePassword,
$Password, and any parameters that have matching values in the custom variables provided.


## Folder synchronization

The synchronizer component will upload all files in a source folder to a destination folder
in SharePoint, taking care of check out, check in, and publishing, as required.

It keeps track of the hash of all files uploaded (stored in a web property bag by default),
and only uploads files if they have changed. The results include the hash codes and
can be used for things like cache-busting references to scripts.

The synchronizer also supports plug-in pre-processors that can do things like replace
"~site/" and "~sitecollection/" tokens, or arbitrary embedded variables, before uploading
the file.

```
var config = new SynchronizerConfiguration();

config.Log = new ConsoleUpgradeLog(true);
config.ContextManager = new BasicContextManager(webUrl, username, password);

var sync = new Synchronizer(config);
var folder = sync.EnsureFolder(destinationFolder);
var result = sync.SynchronizeFolder(sourcePath, destinationFolder);
```

## Examples

The source code includes examples of:

* BasicCodeMigration
* BasicScriptMigration
* BasicSynchronization
* TestApplication -- a more fully formed example with both migrations and sychronization


