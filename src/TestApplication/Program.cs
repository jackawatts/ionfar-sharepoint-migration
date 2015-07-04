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
using IonFar.SharePoint.Migration.Sync;
using System.Linq;

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
//            Console.ReadLine();
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
            var config = new SynchronizerConfiguration();
            config.ContextManager = new BasicContextManager(webUrl, credentials);

            // Store hashes in property bag
            var hashProvider = new WebPropertyHashProvider();
            config.HashProvider = hashProvider;

            // Use NullHashProvider to always upload files
            //config.HashProvider = new NullHashProvider();

            // Will substitute '~site/' and '~sitecollection/'
            config.Preprocessors.Add(new UrlTokenPreprocessor());

            // Will substitute '$key$' with value
            var substitutionVariables = new Dictionary<string, string>();
            substitutionVariables.Add("Message", "Hello");
            config.Preprocessors.Add(new VariableSubstitutionPreprocessor(substitutionVariables));


            var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var scriptsSource = System.IO.Path.Combine(baseFolder, "Scripts");
            var scriptsDestinationFolder = "~site/_catalogs/masterpage/scripts";

            // Ensure folder exists, then synchronise all changed files
            var sync = new Synchronizer(config);
            var folder = sync.EnsureFolder(scriptsDestinationFolder);
            var result = sync.SynchronizeFolder(scriptsSource, scriptsDestinationFolder);

            Console.WriteLine(result.Successful ? "Done" : "Failed");

            // Additional utility function to create a ScriptLink, showing how the results can be used
            var exampleResult = result.Files.First(i => i.ServerRelativeUrl.EndsWith("ionfar.example.js", StringComparison.InvariantCultureIgnoreCase));
            sync.EnsureSiteScriptLink("ScriptLink.ION_Example", exampleResult.ServerRelativeUrl + "?v=" + HttpServerUtility.UrlTokenEncode(exampleResult.Hash), 9999);
        }

    }
}
