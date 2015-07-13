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

            ApplyMigrations(webUrl, username, password);
            SynchroniseFiles(webUrl, username, password);

            Console.WriteLine();
            Console.WriteLine("Finished");
//            Console.ReadLine();
        }

        private static void ApplyMigrations(string webUrl, string username, string password)
        {
            Console.WriteLine();
            Console.WriteLine("= Apply Migrations =");

            var config = new MigratorConfiguration();
            config.Journal = new WebPropertyBagJournal("Test_Migrations/");

            var baseFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var scriptsSource = System.IO.Path.Combine(baseFolder, "Migrations");
            var scriptProvider = new ScriptMigrationProvider(scriptsSource);

            scriptProvider.Variables.Add("Other", Guid.NewGuid());
            config.MigrationProviders.Add(scriptProvider);

            // Use NullJournal to run the migrations every time
            //config.Journal = new NullJournal();

            // Although basic context can accept plain text password, setting only securePassword prevents it being passed through to $SPPassword
            var securePassword = BasicContextManager.GetSecureStringFromString(password);
            config.ContextManager = new BasicContextManager(webUrl, username, securePassword);
            var migrator = new Migrator(config);
            var result = migrator.PerformMigration();

            // Alternative using ExistingContextManager
            //MigrationResult result;
            //var securePassword = BasicContextManager.GetSecureStringFromString(password);
            //var credentials = new SharePointOnlineCredentials(username, securePassword);
            //using (var clientContext = new ClientContext(webUrl))
            //{
            //    clientContext.Credentials = credentials;
            //    // If username + password aren't passed, then $SPContext is available, but not $SPCredentials
            //    config.ContextManager = new ExistingContextManager(clientContext, username, securePassword);
            //    var migrator = new Migrator(config);
            //    result = migrator.PerformMigration();
            //}

            Console.WriteLine(result.Successful ? "Done" : "Failed");
        }

        private static void SynchroniseFiles(string webUrl, string username, string password)
        {
            Console.WriteLine();
            Console.WriteLine("= Synchronise Files =");
            
            var config = new SynchronizerConfiguration();
            config.ContextManager = new BasicContextManager(webUrl, username, password);
            config.HashProvider = new WebPropertyHashProvider("Test_UploadHash", null);

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

            // Use the hash in a script link
            var jqueryResult = result.Files.First(i => i.ServerRelativeUrl.EndsWith("jquery-2.1.4.min.js", StringComparison.InvariantCultureIgnoreCase));
            sync.EnsureSiteScriptLink("ScriptLink.jQuery", "~sitecollection/_catalogs/masterpage/scripts/jquery-2.1.4.min.js", 10000);

            var exampleResult = result.Files.First(i => i.ServerRelativeUrl.EndsWith("ionfar.example.js", StringComparison.InvariantCultureIgnoreCase));
            //            sync.EnsureSiteScriptLink("ScriptLink.IonFar.Example", exampleResult.ServerRelativeUrl + "?v=" + HttpServerUtility.UrlTokenEncode(exampleResult.Hash), 10100);
            sync.EnsureSiteScriptLink("ScriptLink.IonFar.Example", "~sitecollection/_catalogs/masterpage/scripts/ionfar.example.js?v=" + HttpServerUtility.UrlTokenEncode(exampleResult.Hash), 10100);

        }


    }
}
