using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Journals;
using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Providers.Script;
using IonFar.SharePoint.Migration.Services;
using System.Management.Automation;

namespace IonFar.SharePoint.PowerShell
{
    /// <summary>
    /// <para type="synopsis">Runs IonFar migration scripts</para>
    /// <para type="description">Runs a series of deployment scripts against a provided SharePoint Online site.</para>
    /// <para type="description">By default, runs only the new scripts that weren't executed before.</para>
    /// <para type="description">This behaviour can be overriden by using -Force parameter.</para>
    /// <code>Invoke-IonFarScriptMigration -SiteUrl $SiteUrl -UserName $UserName -Password $Password -ScriptDirectory "$($PSScriptRoot)\Migrations"</code>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "IonFarScriptMigration")]
    [OutputType(typeof(MigrationResult))]
    public class InvokeScriptMigrationCmdlet: Cmdlet
    {
        /// <summary>
        /// <para type="description">The url of the target SharePoint site</para>
        /// </summary>
        [Parameter(Mandatory = true, 
            ValueFromPipeline = true,
            HelpMessage = "Target SharePoint site/web url")]
        public string SiteUrl { get; set; }

        /// <summary>
        /// <para type="description">User account to run the migrations</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "User account to run the migrations")]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Password of the user account to run the migrations</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "Password of the user account")]
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">Full path to scripts directory</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "Full path to scripts directory")]
        public string ScriptDirectory { get; set; }

        /// <summary>
        /// <para type="description">Runs all scripts even if they were executed before</para>
        /// </summary>
        [Parameter(HelpMessage ="Runs all scripts even if they were executed before")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// <para type="description">Specify a custom property bag prefix</para>
        /// </summary>
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
