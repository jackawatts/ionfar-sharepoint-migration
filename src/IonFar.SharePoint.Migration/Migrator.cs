using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IonFar.SharePoint.Migration
{
    public class Migrator : IMigrator
    {
        private readonly ClientContext _clientContext;
        private readonly MigratorConfiguration _configuration;

        public Migrator(ClientContext clientContext, MigratorConfiguration configuration)
        {
            _configuration = configuration;
            _clientContext = clientContext;
        }

        /// <summary>
        /// Migrates all IMigrations within the given assembly.
        /// </summary>
        /// <param name="assemblyContainingMigrations">The assembly containing the migrations to run.</param>
        /// <remarks>Migrations must be decorated with a <see cref="MigrationAttribute"/></remarks>
        public void Migrate(Assembly assemblyContainingMigrations)
        {
            Migrate(assemblyContainingMigrations, name => true);
        }

        /// <summary>
        /// Migrates all IMigrations within the given assembly matching the given filter given the Type.FullName.
        /// </summary>
        /// <param name="assemblyContainingMigrations">The assembly containing the migrations to run.</param>
        /// <param name="filter">A function that accepts a Type.FullName and returns true if the given type is to be migrated, otherwise false.</param>
        /// <remarks>Migrations must be decorated with a <see cref="MigrationAttribute"/></remarks>
        public void Migrate(Assembly assemblyContainingMigrations, Func<string, bool> filter)
        {
            try
            {
                _configuration.Log.Information("Starting upgrade against SharePoint instance at " + _clientContext.Url);
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                _configuration.Log.Information("IonFar.SharePoint.Migrator v" + fvi.FileVersion);

                var availableMigrations = GetAvailableMigrations(assemblyContainingMigrations, filter);
                var appliedMigrations = GetAppliedMigrations(availableMigrations);
                var migrationsToRun = GetMigrationsToRun(appliedMigrations, availableMigrations);

                if (!availableMigrations.Any())
                {
                    _configuration.Log.Information("There are no migrations available in the specified assembly: " +
                            assemblyContainingMigrations.FullName);
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

        private MigrationInfo[] GetAvailableMigrations(Assembly assemblyContainingMigrations, Func<string, bool> filter)
        {
            var availableMigrations =
                assemblyContainingMigrations
                    .GetExportedTypes()
                    .Where(candidateType => typeof(IMigration).IsAssignableFrom(candidateType) &&
                        !candidateType.IsAbstract &&
                        candidateType.IsClass &&
                        filter(candidateType.FullName)
                        )
                    .Select(candidateType => new MigrationInfo(candidateType))
                    .Where(migrationinfo => migrationinfo.Version > 0)
                    .ToArray();

            return availableMigrations;
        }
    }
}