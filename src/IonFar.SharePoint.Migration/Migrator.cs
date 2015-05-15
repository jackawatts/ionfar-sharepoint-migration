using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IonFar.SharePoint.Migration
{
    public class Migrator
    {
        private readonly MigratorConfiguration _configuration;

        public Migrator(MigratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Executes the migration.
        /// </summary>
        public void PerformMigration()
        {
            _configuration.Validate();
            try
            {
                using (_configuration.ContextManager.ContextScope(_configuration.Log))
                {
                    var clientContext = _configuration.ContextManager.CurrentContext;

                    var assembly = Assembly.GetExecutingAssembly();
                    var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _configuration.Log.Information("IonFar.SharePoint.Migrator v" + fvi.FileVersion);
                    _configuration.Log.Information("Starting upgrade against SharePoint instance at " + clientContext.Url);

                    var availableMigrations = _configuration.MigrationProviders.SelectMany(provider => provider.GetMigrations(_configuration.ContextManager, _configuration.Log));

                    var appliedMigrations = _configuration.Journal.GetExecutedMigrations(_configuration.ContextManager, _configuration.Log);

                    var migrationsToRun = availableMigrations.Where(available => !appliedMigrations.Any(applied => applied.Name.Equals(available.Name, StringComparison.InvariantCultureIgnoreCase)));

                    //if (!availableMigrations.Any())
                    //{
                    //    _configuration.Log.Information("There are no migrations from any of the providers.");
                    //    return;
                    //}

                    if (!migrationsToRun.Any())
                    {
                        _configuration.Log.Information("The SharePoint instance is up to date, there are no migrations to run.");
                        return;
                    }

                    foreach (var migration in migrationsToRun)
                    {
                        _configuration.Log.Information(string.Format("Upgrading by running '{0}'", migration.Name));

                        migration.Apply(_configuration.ContextManager, _configuration.Log);

                        _configuration.Journal.StoreExecutedMigration(_configuration.ContextManager, _configuration.Log, migration);

                        _configuration.Log.Information("The migration is complete.");

                    }
                }
            }
            catch (Exception ex)
            {
                _configuration.Log.Error(
                    "The migration failed and the environment has been left in a partially complete state, manual intervention may be required.\nException: {0}", ex
                );
                throw;
            }
        }

    }
}