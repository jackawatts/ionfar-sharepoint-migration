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
            var config = new MigratorConfiguration() {};
            //_logger = new ConsoleUpgradeLog();
            //config.Log = new TraceUpgradeLog();

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

            config.ContextManager = new BasicContextManager(webUrl, credentials);

            config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetAssembly(typeof(ShowTitle))));
            //config.Journal = new NullJournal();

            var migrator = new Migrator(config);

            migrator.Migrate();

            //using (var clientContext = new ClientContext(webUrl))
            //{
            //}

            Console.WriteLine("Done");
            Console.ReadLine();
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
