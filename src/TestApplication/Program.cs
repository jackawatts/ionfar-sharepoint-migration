using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Providers;
using IonFar.SharePoint.Migration.Services;
using IonFar.SharePoint.Migration.Sync;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Web;
using TestApplication.Migrations;

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

            //TestBasicMigration(webUrl, username, password);
            //TestFolderUpload(webUrl, username, password);
            TestScriptMigration(webUrl, username, password);

            Console.WriteLine();
            Console.WriteLine("Finished");
//            Console.ReadLine();
        }


        private static void TestBasicMigration(string webUrl, string username, string password)
        {
            Console.WriteLine();
            Console.WriteLine("# TestBasicMigration #");

            var config = new MigratorConfiguration();
            // Use ConsoleUpgradeLog for coloured console output, 
            // or use something like ColoreConsoleTraceListener from Essential.Diagnostics
            //config.Log = new ConsoleUpgradeLog();

            config.MigrationProviders.Add(new AssemblyMigrationProvider(Assembly.GetAssembly(typeof(ShowTitle))));

            // Use NullJournal to run the migrations every time
            //config.Journal = new NullJournal();

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

        private static void TestFolderUpload(string webUrl, string username, string password)
        {
            Console.WriteLine();
            Console.WriteLine("# TestFolderUpload #");
            
            var config = new SynchronizerConfiguration();
            config.ContextManager = new BasicContextManager(webUrl, username, password);


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
        
        private static void TestScriptMigration(string webUrl, string username, string password)
        {
            Console.WriteLine();
            Console.WriteLine("# TestScriptMigration #");

            var config = new MigratorConfiguration();
            // Use ConsoleUpgradeLog for coloured console output, 
            // or use something like ColoreConsoleTraceListener from Essential.Diagnostics
            //config.Log = new ConsoleUpgradeLog();

            var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var scriptsSource = System.IO.Path.Combine(baseFolder, "Migrations");
            config.MigrationProviders.Add(new ScriptMigrationProvider(scriptsSource));

            // Use NullJournal to run the migrations every time
            config.Journal = new NullJournal();

            //config.ContextManager = new BasicContextManager(webUrl, username, password);
            //var migrator = new Migrator(config);
            //var result = migrator.PerformMigration();

            // Alternative using ExistingContextManager
            MigrationResult result;
            SecureString securePassword = BasicContextManager.GetSecureStringFromString(password);
            ICredentials credentials = new SharePointOnlineCredentials(username, securePassword);
            using (var clientContext = new ClientContext(webUrl))
            {
                clientContext.Credentials = credentials;
                config.ContextManager = new ExistingContextManager(clientContext, username, securePassword);
                var migrator = new Migrator(config);
                result = migrator.PerformMigration();
            }

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }

    }
}
