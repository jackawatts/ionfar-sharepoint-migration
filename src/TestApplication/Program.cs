using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Infrastructure;
using Microsoft.SharePoint.Client;
using TestApplication.Migrations;

namespace TestApplication
{
    internal class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            _logger = new ConsoleLogger(); 
            string webUrl = "";
            string userName = "";
            SecureString password = GetSecureStringFromString("");

            using (var clientContext = new ClientContext(webUrl))
            {
                clientContext.Credentials = new SharePointOnlineCredentials(userName, password);

                var migrator = new Migrator(clientContext, _logger);
                migrator.Migrate(Assembly.GetAssembly(typeof(ShowTitle)));
            }
        }

        private static SecureString GetSecureStringFromString(string pass)
        {
            var password = new SecureString();
            foreach (var c in pass.ToCharArray())
            {
                password.AppendChar(c);
            }

            return password;
        }
    }
}
