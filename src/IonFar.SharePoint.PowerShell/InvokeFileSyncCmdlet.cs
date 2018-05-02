using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Services;
using IonFar.SharePoint.Synchronization;
using System;
using System.IO;
using System.Management.Automation;

namespace IonFar.SharePoint.PowerShell
{
    /// <summary>
    /// <para type="synopsis">Runs IonFar file synchronisation</para>
    /// <para type="description">Synchronizes files from given source directory to target SharePoint folder.</para>
    /// <para type="description">Transfers only if the local file has changed, by comparing the hashes.</para>
    /// <code>Invoke-IonFarFileSync -SiteUrl $SiteUrl -UserName $UserName -Password $Password -BaseDirectory "$($PSScriptRoot)" -SourcePath "Files\Style Library" -DestinationPath "~site/Style Library"</code>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "IonFarFileSync")]
    [OutputType(typeof(SynchronizationResult))]
    public class InvokeFileSyncCmdlet : Cmdlet
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
        /// <para type="description">Full path to base working directory. Defaults to current directory if not specified</para>
        /// </summary>
        [Parameter(HelpMessage = "Full path to base working directory. Defaults to current directory if not specified")]
        public string BaseDirectory { get; set; }

        /// <summary>
        /// <para type="description">Source folder path relative to working directory</para>
        /// </summary>
        [Parameter(Mandatory = true, 
            HelpMessage = "Source folder path relative to working directory")]
        public string SourcePath { get; set; }

        /// <summary>
        /// <para type="description">Server relative destination SharePoint folder path. May use '~sitecollection/' or '~site/' prefix; the folder must already exist</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "Server relative destination SharePoint folder path. May use '~sitecollection/' or '~site/' prefix; the folder must already exist")]
        public string DestinationPath { get; set; }

        protected override void ProcessRecord()
        {
            var config = new SynchronizerConfiguration
            {
                Log = new ConsoleUpgradeLog(true),
                ContextManager = new BasicContextManager(this.SiteUrl, this.UserName, this.Password)
            };

            var baseFolder = this.BaseDirectory;
            if (String.IsNullOrEmpty(baseFolder))
            {
                baseFolder = Directory.GetCurrentDirectory();
            }
            var sourcePath = System.IO.Path.Combine(baseFolder, this.SourcePath);

            var sync = new Synchronizer(config);
            var result = sync.SynchronizeFolder(sourcePath, this.DestinationPath);

            WriteObject(result);
        }
    }
}
