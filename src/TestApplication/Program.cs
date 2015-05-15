using System.Reflection;
using System.Security;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;
using TestApplication.Migrations;
using System;
using IonFar.SharePoint.Migration.Providers;
using System.Net;

namespace TestApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: TestApplication.exe username password sitecollectionurl");
                return;
            }
            string username = args[0];
            string password = args[1];
            string webUrl = args[2];
            SecureString securePassword = GetSecureStringFromString(password);
            ICredentials credentials = new SharePointOnlineCredentials(username, securePassword);

            var config = new MigratorConfiguration();
            // Use ConsoleUpgradeLog for coloured console output, 
            // or use something like ColoreConsoleTraceListener from Essential.Diagnostics
            //config.Log = new ConsoleUpgradeLog();

            config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetAssembly(typeof(ShowTitle))));

            // Use NullJournal to run the migrations every time
            //config.Journal = new NullJournal();

            config.ContextManager = new BasicContextManager(webUrl, credentials);
            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();
            Console.WriteLine(result.Successful ? "Done" : "Failed");

            // Alternative using ExistingContextManager
            //MigrationResult result;
            //using (var clientContext = new ClientContext(webUrl))
            //{
            //    clientContext.Credentials = credentials;
            //    config.ContextManager = new ExistingContextManager(clientContext);
            //    var migrator = new Migrator(config);
            //    result = migrator.PerformMigration();
            //}

            Console.ReadLine();
            if (!result.Successful)
            {
                Environment.Exit(9);
            }
        }

        private static SecureString GetSecureStringFromString(string nonsecureString)
        {
            var result = new SecureString();
            foreach (char c in nonsecureString)
            {
                result.AppendChar(c);
            }

            return result;
        }
    }
}
