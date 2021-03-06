﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace IonFar.SharePoint.Migration.Journals
{
    /// <summary>
    /// A journal that tracks executed migrations as property bag values with a specific prefix.
    /// </summary>
    public class WebPropertyBagJournal : IJournal
    {
        public const string DefaultPrefix = "ION_Migration/";

        private readonly string _prefix;
        long _lastId = -1;
        bool _lastIdInitialised;

        /// <summary>
        /// Creates a property bag journal with the default prefix.
        /// </summary>
        public WebPropertyBagJournal()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a property bag journal with the specified prefix.
        /// </summary>
        /// <param name="prefix">Property key prefix; if null the default prefix (ION_Migration/) is used</param>
        public WebPropertyBagJournal(string prefix)
        {
            _prefix = prefix ?? DefaultPrefix;
        }

        /// <summary>
        /// Gets the migrations that have already been executed.
        /// </summary>
        /// <param name="contextManager">The Context Manager</param>
        /// <param name="log">The UpgradeLog.</param>
        public IEnumerable<MigrationInfo> GetExecutedMigrations(IContextManager contextManager, IUpgradeLog log)
        {
            var executedMigrations = new List<MigrationInfo>();

            var clientContext = contextManager.CurrentContext;
            var rootWeb = clientContext.Site.RootWeb;
            clientContext.Load(rootWeb);
            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);
            clientContext.ExecuteQuery();

            var appliedMigrationProperties = properties.FieldValues.Where(f => f.Key.StartsWith(_prefix));
            _lastId = 0;
            _lastIdInitialised = true;
            foreach (var migrationProperty in appliedMigrationProperties)
            {
                MigrationInfo migrationInfo = null;
                try
                {
                    migrationInfo = JsonConvert.DeserializeObject(migrationProperty.Value.ToString(), typeof(MigrationInfo)) as MigrationInfo;
                    log.Verbose("Retrieved migration info ID {0} '{1}'", migrationInfo.Id, migrationInfo.Name);
                }
                catch (Exception ex)
                {
                    log.Verbose("Exception suppressed deserializing migration info: {0}", ex);
                }
                if (migrationInfo == null)
                {
                    var idString = migrationProperty.Key.Substring(_prefix.Length);
                    long idFromProperty;
                    if (long.TryParse(idString, out idFromProperty) && idFromProperty > 0)
                    {
                        log.Warning("Defaulting to property key value '{0}' (ID and Name), due to deserialization problem (details in Verbose log).", idFromProperty);
                        migrationInfo = new MigrationInfo(idFromProperty, idFromProperty.ToString(), null, DateTimeOffset.MinValue);
                    }
                }
                if (migrationInfo == null)
                {
                    log.Warning("Skipping property key '{0}' due to parsing problem, but continuing. Details in Verbose log.", migrationProperty.Key);
                }
                else
                { 
                    if (migrationInfo.Id > _lastId)
                    {
                        _lastId = migrationInfo.Id;
                    }
                    executedMigrations.Add(migrationInfo);
                    //yield return migrationInfo;
                }
            }
            return executedMigrations;
        }

        /// <summary>
        /// Records a migration as having been run.
        /// </summary>
        /// <param name="contextManager">The ContextManager</param>
        /// <param name="log">The UpgradeLog</param>
        /// <param name="migration">Migration that has been run</param>
        public MigrationInfo StoreExecutedMigration(IContextManager contextManager, IUpgradeLog log, IMigration migration)
        {
            if (!_lastIdInitialised)
            {
                throw new InvalidOperationException("GetExecutedMigrations (to get previous migrations) is expected before StoreExecutedMigration (to store new ones)");
            }

            var clientContext = contextManager.CurrentContext;
            var rootWeb = clientContext.Site.RootWeb;
            clientContext.Load(rootWeb);
            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);
            clientContext.ExecuteQuery();

            var id = _lastId + 1;
            var migrationInfo = new MigrationInfo(id, migration.Name, migration.Note, DateTimeOffset.UtcNow);
            var key = _prefix + migrationInfo.Id;
            properties[key] = JsonConvert.SerializeObject(migrationInfo);

            rootWeb.Update();
            log.Verbose("Storing migration info '{0}'", key);
            clientContext.ExecuteQuery();

            _lastId = id;

            return migrationInfo;
        }
    }
}
