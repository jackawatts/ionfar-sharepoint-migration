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
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public string SiteUrl { get; set; }

        [Parameter(Mandatory = true)]
        public string UserName { get; set; }

        [Parameter(Mandatory = true)]
        public string Password { get; set; }

        [Parameter]
        public string BaseDirectory { get; set; }

        [Parameter(Mandatory = true)]
        public string SourcePath { get; set; }

        [Parameter(Mandatory = true)]
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
