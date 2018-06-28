﻿using IonFar.SharePoint.Migration;
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
        [Parameter(Mandatory = false,
            HelpMessage = "User account to run the migrations")]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Password of the user account to run the migrations</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Password of the user account")]
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">App Principal Client ID</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "App Principal client id for authentication")]
        public string ClientId { get; set; }

        /// <summary>
        /// <para type="description">App Principal Client Secret</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "App principal client secret for authentication")]
        public string ClientSecret { get; set; }

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
            if(string.IsNullOrEmpty(this.UserName) && string.IsNullOrEmpty(this.ClientId))
            {
                throw new System.Exception("Either UserName/Password or ClientId/ClientSecret must provided for authentication");
            }

            var config = new MigratorConfiguration
            {
                Log = new ConsoleUpgradeLog(true),
                Journal = this.Force ? (IJournal)new NullJournal() : (IJournal)new WebPropertyBagJournal()
            };

            if(string.IsNullOrEmpty(this.ClientId))
            {
                config.ContextManager = new BasicContextManager(this.SiteUrl, this.UserName, this.Password);
            }
            else
            {
                config.ContextManager = new AppOnlyContextManager(this.SiteUrl, this.ClientId, this.ClientSecret);
            }

            config.MigrationProviders.Add(new ScriptMigrationProvider(this.ScriptDirectory));

            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            WriteObject(result);
        }
    }
}
