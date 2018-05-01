using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Journals;
using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Providers.Script;
using IonFar.SharePoint.Migration.Services;
using System;
using System.Management.Automation;

namespace IonFar.SharePoint.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptMigration")]
    [OutputType(typeof(MigrationResult))]
    public class InvokeScriptMigrationCmdlet: Cmdlet
    {
        [Parameter(Mandatory = true, 
            ValueFromPipeline = true,
            HelpMessage = "Target SharePoint site/web url")]
        public string SiteUrl { get; set; }

        [Parameter(Mandatory = true,
            HelpMessage = "User account to run the migrations")]
        public string UserName { get; set; }

        [Parameter(Mandatory = true,
            HelpMessage = "Password of the user account")]
        public string Password { get; set; }

        [Parameter(Mandatory = true,
            HelpMessage = "Full path to scripts directory")]
        public string ScriptDirectory { get; set; }

        [Parameter(HelpMessage ="Runs all scripts even if they were executed before")]
        public SwitchParameter Force { get; set; }

        [Parameter(HelpMessage = "Specify a custom property bag prefix")]
        public string JournalPrefix { get; set; }

        protected override void ProcessRecord()
        {
            var config = new MigratorConfiguration
            {
                Log = new ConsoleUpgradeLog(true),
                Journal = this.Force ? (IJournal)new NullJournal() : (IJournal)new WebPropertyBagJournal(),
                ContextManager = new BasicContextManager(this.SiteUrl, this.UserName, this.Password)
            };

            config.MigrationProviders.Add(new ScriptMigrationProvider(this.ScriptDirectory));

            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            WriteObject(result);
        }
    }
}
