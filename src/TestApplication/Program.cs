using System.Reflection;
using System.Security;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;
using TestApplication.Migrations;
using IonFar.SharePoint.Migration.Output;
using System;

namespace TestApplication
{
    internal class Program
    {
        private static IUpgradeLog _logger;

        private static void Main(string[] args)
        {
            //_logger = new ConsoleUpgradeLog();
            _logger = new TraceUpgradeLog();

            if (args.Length != 3)
            {
                _logger.Warning("Format is: TestApplication.exe username password sitecollectionurl");
                return;
            }

            string username = args[0];
            string password = args[1];
            string webUrl = args[2];

            SecureString securePassword = GetSecureStringFromString(password);

            using (var clientContext = new ClientContext(webUrl))
            {
                clientContext.Credentials = new SharePointOnlineCredentials(username, securePassword);

                var migrator = new Migrator(clientContext, new MigratorConfiguration() { Log = _logger });
                migrator.Migrate(Assembly.GetAssembly(typeof(ShowTitle)));
            }

            //Console.ReadLine();
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
