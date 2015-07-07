using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Services;
using IonFar.SharePoint.Migration.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicSynchronization
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: BasicSynchronization.exe sitecollectionurl username password");
                return;
            }
            string webUrl = args[0];
            string username = args[1];
            string password = args[2];

            var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var sourcePath = System.IO.Path.Combine(baseFolder, @"Files\Style Library");
            var destinationFolder = "~site/style library";

            var config = new SynchronizerConfiguration();

            config.Log = new ConsoleUpgradeLog(true);
            config.ContextManager = new BasicContextManager(webUrl, username, password);

            var sync = new Synchronizer(config);
            var result = sync.SynchronizeFolder(sourcePath, destinationFolder);

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }
    }
}
