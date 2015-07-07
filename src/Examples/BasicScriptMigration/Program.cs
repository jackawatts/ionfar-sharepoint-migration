using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Journals;
using IonFar.SharePoint.Migration.Logs;
using IonFar.SharePoint.Migration.Providers.Script;
using IonFar.SharePoint.Migration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicScriptMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: BasicScriptMigration.exe sitecollectionurl username password");
                return;
            }
            string webUrl = args[0];
            string username = args[1];
            string password = args[2];
            var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var scriptsSource = System.IO.Path.Combine(baseFolder, "Migrations");

            var config = new MigratorConfiguration();

            config.Log = new ConsoleUpgradeLog(true); // Alternatively use ColoreConsoleTraceListener from Essential.Diagnostics
            config.Journal = new NullJournal(); // Use NullJournal to run the migrations every time
            config.MigrationProviders.Add(new ScriptMigrationProvider(scriptsSource));
            config.ContextManager = new BasicContextManager(webUrl, username, password);

            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }
    }
}
