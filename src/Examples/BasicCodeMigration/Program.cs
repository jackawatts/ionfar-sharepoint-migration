using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Providers;
using IonFar.SharePoint.Migration.Services;
using System;
using System.Reflection;
using BasicCodeMigration.Migrations;

namespace BasicCodeMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: BasicCodeMigration.exe sitecollectionurl username password");
                return;
            }
            string webUrl = args[0];
            string username = args[1];
            string password = args[2];

            var config = new MigratorConfiguration();       
                 
            config.Log = new ConsoleUpgradeLog(true); // Alternatively use ColoreConsoleTraceListener from Essential.Diagnostics
            config.Journal = new NullJournal(); // Use NullJournal to run the migrations every time
            config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetAssembly(typeof(ShowTitle))));
            config.ContextManager = new BasicContextManager(webUrl, username, password);

            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }
    }
}
