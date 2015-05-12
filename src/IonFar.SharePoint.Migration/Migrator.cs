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
        public void Migrate()
        {
            _configuration.Validate();
            try
            {
                using (_configuration.ContextManager.ContextScope(_configuration.Log))
                {
                    var clientContext = _configuration.ContextManager.CurrentContext;

                    _configuration.Log.Information("Starting upgrade against SharePoint instance at " + clientContext.Url);
                    var assembly = Assembly.GetExecutingAssembly();
                    var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    _configuration.Log.Information("IonFar.SharePoint.Migrator v" + fvi.FileVersion);

                    var availableMigrations = _configuration.MigrationProviders.SelectMany(provider => provider.GetMigrations());

                    var appliedMigrations = _configuration.Journal.GetExecutedMigrations(_configuration.ContextManager, _configuration.Log);

                    var migrationsToRun = GetMigrationsToRun(appliedMigrations, availableMigrations);

                    if (!availableMigrations.Any())
                    {
                        _configuration.Log.Information("There are no migrations from any of the providers.");
                        return;
                    }

                    if (!migrationsToRun.Any())
                    {
                        _configuration.Log.Information("The SharePoint instance is up to date, there are no migrations to run.");
                        return;
                    }

                    foreach (var migrationInfo in migrationsToRun)
                    {
                        _configuration.Log.Information(string.Format("Upgrading to {0} by running {1}...",
                            migrationInfo.Version,
                            migrationInfo.FullName));

                        migrationInfo.ApplyMigration(_configuration.ContextManager, _configuration.Log);

                        _configuration.Journal.StoreExecutedMigration(_configuration.ContextManager, _configuration.Log, migrationInfo);

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

        private MigrationInfo[] GetMigrationsToRun(IEnumerable<MigrationInfo> appliedMigrations, IEnumerable<MigrationInfo> availableMigrations)
        {
            var appliedVersions = new HashSet<long>(appliedMigrations.Select(m => m.Version));

            return availableMigrations
                .Where(availableMigration => !appliedVersions.Contains(availableMigration.Version) 
                    || availableMigration.OverrideCurrentDeployment)
                .OrderBy(migrationToRun => migrationToRun.Version)
                .ToArray();
        }

    }
}