using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
//using System.Diagnostics;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Applies incremental migrations to a SharePoint site.
    /// </summary>
    public class Migrator
    {
        private readonly MigratorConfiguration _configuration;

        /// <summary>
        /// Creates a Migrator with the specified configuration
        /// </summary>
        /// <param name="configuration">the configuration</param>
        public Migrator(MigratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Executes the migration.
        /// </summary>
        public MigrationResult PerformMigration()
        {
            _configuration.Validate();
            var appliedMigrations = new List<MigrationInfo>();
            try
            {
                using (_configuration.ContextManager.ContextScope(_configuration.Log))
                {
                    var clientContext = _configuration.ContextManager.CurrentContext;

                    var assembly = Assembly.GetExecutingAssembly();
                    var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _configuration.Log.Information("IonFar SharePoint Migrator v" + fvi.FileVersion);
                    _configuration.Log.Information("Starting upgrade against SharePoint instance at " + clientContext.Url);

                    var availableMigrations = _configuration.MigrationProviders.SelectMany(provider => provider.GetMigrations(_configuration.ContextManager, _configuration.Log));

                    var previousMigrations = _configuration.Journal.GetExecutedMigrations(_configuration.ContextManager, _configuration.Log);

                    var migrationsToRun = availableMigrations.Where(available => !previousMigrations.Any(previous => previous.Name.Equals(available.Name, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    if (migrationsToRun.Count == 0)
                    {
                        _configuration.Log.Information("The SharePoint instance is up to date, there are no migrations to run.");
                        return new MigrationResult(appliedMigrations, successful: true, error: null);
                    }

                    foreach (var migration in migrationsToRun)
                    {
                        _configuration.Log.Information(string.Format("== Upgrading by running migration '{0}' ==", migration.Name));

                        migration.Apply(_configuration.ContextManager, _configuration.Log);

                        var migrationInfo =_configuration.Journal.StoreExecutedMigration(_configuration.ContextManager, _configuration.Log, migration);

                        _configuration.Log.Verbose("Migration '{0}' complete (journal ID: {1})", migrationInfo.Name, migrationInfo.Id);

                        appliedMigrations.Add(migrationInfo);
                    }

                    _configuration.Log.Information("Migration run successful");
                    return new MigrationResult(appliedMigrations, successful: true, error: null);
                }
            }
            catch (Exception ex)
            {
                _configuration.Log.Critical(
                    "Migration failed and the environment has been left in a partially complete state, manual intervention may be required.\nException: {0}", ex
                );
                return new MigrationResult(appliedMigrations, successful: false, error: ex);
            }
        }

    }
}