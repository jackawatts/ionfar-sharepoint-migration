using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Journals;
using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Providers.Script;
using IonFar.SharePoint.Migration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptMigration")]
    [OutputType(typeof(MigrationResult))]
    public class InvokeScriptMigrationCmdlet: Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public string SiteUrl { get; set; }

        [Parameter(Mandatory = true)]
        public string UserName { get; set; }

        [Parameter(Mandatory = true)]
        public string Password { get; set; }

        [Parameter(Mandatory = true)]
        public string ScriptDirectory { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string WorkingDirectory { get; set; }

        [Parameter]
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
