using System.Reflection;
using System.Security;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;
using TestApplication.Migrations;
using System;
using IonFar.SharePoint.Migration.Providers;
using System.Net;
using IonFar.SharePoint.Migration.Services;
using System.Collections.Generic;
using System.Web;

namespace TestApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Format is: TestApplication.exe sitecollectionurl username password");
                return;
            }
            string webUrl = args[0];
            string username = args[1];
            string password = args[2];
            SecureString securePassword = GetSecureStringFromString(password);
            ICredentials credentials = new SharePointOnlineCredentials(username, securePassword);

            TestBasicMigration(webUrl, credentials);
            TestFolderUpload(webUrl, credentials);

            Console.WriteLine("Finished");
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

        private static void TestBasicMigration(string webUrl, ICredentials credentials)
        {
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

            // Alternative using ExistingContextManager
            //MigrationResult result;
            //using (var clientContext = new ClientContext(webUrl))
            //{
            //    clientContext.Credentials = credentials;
            //    config.ContextManager = new ExistingContextManager(clientContext);
            //    var migrator = new Migrator(config);
            //    result = migrator.PerformMigration();
            //}

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }

        private static void TestFolderUpload(string webUrl, ICredentials credentials)
        {
            using (var clientContext = new ClientContext(webUrl))
            {
                clientContext.Credentials = credentials;

                var uploadService = new FileUploadService(clientContext);
                uploadService.HashProvider = new WebPropertyHashProvider(clientContext.Site, clientContext.Site.RootWeb);
                uploadService.Preprocessors.Add(new UrlTokenPreprocessor(clientContext));
                var substitutionVariables = new Dictionary<string, string>();
                substitutionVariables.Add("Message", "Hello");
                uploadService.Preprocessors.Add(new VariableSubstitutionPreprocessor(substitutionVariables));

                var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                var scriptsSource = System.IO.Path.Combine(baseFolder, "Scripts");
                var scriptsDestinationFolder = "~site/_catalogs/masterpage/scripts";
                uploadService.EnsureFolder(scriptsDestinationFolder);
                uploadService.UploadFolder(scriptsSource, scriptsDestinationFolder);

                var hash = uploadService.HashProvider.GetFileHash("~sitecollection/_catalogs/masterpage/scripts/ionfar.example.js");
                uploadService.EnsureSiteScriptLink("ScriptLink.ION_Example", "~sitecollection/_catalogs/masterpage/scripts/ionfar.example.js?v=" + HttpServerUtility.UrlTokenEncode(hash), 10100);
            }
        }

    }
}
