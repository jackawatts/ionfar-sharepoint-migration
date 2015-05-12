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
        private readonly ClientContext _clientContext;
        private readonly MigratorConfiguration _configuration;

        public Migrator(ClientContext clientContext, MigratorConfiguration configuration)
        {
            _configuration = configuration;
            _clientContext = clientContext;
        }

        /// <summary>
        /// Executes the migration.
        /// </summary>
        public void Migrate()
        {
            try
            {
                _configuration.Log.Information("Starting upgrade against SharePoint instance at " + _clientContext.Url);
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                _configuration.Log.Information("IonFar.SharePoint.Migrator v" + fvi.FileVersion);

                var availableMigrations = _configuration.MigrationProviders.SelectMany(provider => provider.GetMigrations());

                var appliedMigrations = GetAppliedMigrations(availableMigrations.ToArray());
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

                    migrationInfo.ApplyMigration(_clientContext, _configuration.Log);
                    var rootWeb = _clientContext.Site.RootWeb;

                    _clientContext.Load(rootWeb);

                    var properties = rootWeb.AllProperties;
                    _clientContext.Load(properties);
                    _clientContext.ExecuteQuery();

                    properties[migrationInfo.Id] = JsonConvert.SerializeObject(migrationInfo);
                    
                    rootWeb.Update();
                    _clientContext.ExecuteQuery();
                    _configuration.Log.Information("The migration is complete.");

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

        private MigrationInfo[] GetAppliedMigrations(MigrationInfo[] availableMigrations)
        {
            var availableMigrationsIds = availableMigrations.Select(am => am.Id).ToList();
            var rootWeb = _clientContext.Site.RootWeb;

            _clientContext.Load(rootWeb);

            var properties = rootWeb.AllProperties;
            _clientContext.Load(properties);

            _clientContext.ExecuteQuery();

            var appliedMigrations = properties.FieldValues.Where(f => f.Key.StartsWith(MigrationInfo.Prefix))
                .Where(f => availableMigrationsIds.Contains(f.Key))
                .Select(f => JsonConvert.DeserializeObject(f.Value.ToString(), typeof(MigrationInfo)) as MigrationInfo);

            return appliedMigrations.ToArray();
        }
    }
}