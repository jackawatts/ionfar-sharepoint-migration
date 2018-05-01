using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Services;
using IonFar.SharePoint.Synchronization;
using System;
using System.IO;
using System.Management.Automation;

namespace IonFar.SharePoint.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "FileSync")]
    [OutputType(typeof(SynchronizationResult))]
    public class InvokeFileSyncCmdlet : Cmdlet
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

        [Parameter(HelpMessage = "Full path to base working directory. Defaults to current directory if not specified")]
        public string BaseDirectory { get; set; }

        [Parameter(Mandatory = true, 
            HelpMessage = "Source folder path relative to working directory")]
        public string SourcePath { get; set; }

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
