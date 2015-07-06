using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Providers;
using IonFar.SharePoint.Migration.Services;
using System;
using System.Reflection;
using TestApplication.Migrations;

namespace BasicMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: BasicMigration.exe sitecollectionurl username password");
                return;
            }
            string webUrl = args[0];
            string username = args[1];
            string password = args[2];

            var config = new MigratorConfiguration();

            // Simple coloured logging to the console
            // Alternatively use ColoreConsoleTraceListener from Essential.Diagnostics
            config.Log = new ConsoleUpgradeLog();

            // Use NullJournal to run the migrations every time
            config.Journal = new NullJournal();

            // Add the migrations
            config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetAssembly(typeof(ShowTitle))));

            config.ContextManager = new BasicContextManager(webUrl, username, password);
            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            // Alternative using ExistingContextManager
            //MigrationResult result;
            //SecureString securePassword = BasicContextManager.GetSecureStringFromString(password);
            //ICredentials credentials = new SharePointOnlineCredentials(username, securePassword);
            //using (var clientContext = new ClientContext(webUrl))
            //{
            //    clientContext.Credentials = credentials;
            //    config.ContextManager = new ExistingContextManager(clientContext, null, null);
            //    var migrator = new Migrator(config);
            //    result = migrator.PerformMigration();
            //}

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }
    }
}
