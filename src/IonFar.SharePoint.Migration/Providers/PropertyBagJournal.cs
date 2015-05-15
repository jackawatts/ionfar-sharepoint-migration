using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    /// <summary>
    /// A journal that tracks executed migrations as property bag values with a specific prefix.
    /// </summary>
    public class PropertyBagJournal : IJournal
    {
        public const string DefaultPrefix = "migrationinfos/";

        string _prefix;
        long _lastId = -1;
        bool _lastIdInitialised;

        /// <summary>
        /// Creates a property bag journal with the default prefix.
        /// </summary>
        public PropertyBagJournal()
            : this(DefaultPrefix)
        {
        }

        /// <summary>
        /// Creates a property bag journal with the specified prefix.
        /// </summary>
        /// <param name="prefix">Property key prefix</param>
        public PropertyBagJournal(string prefix)
        {
            _prefix = prefix;
        }

        /// <summary>
        /// Gets the migrations that have already been executed.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
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
        /// <param name="clientContext">Context to the SharePoint server</param>
        /// <param name="migration">Migration that has been run</param>
        public void StoreExecutedMigration(IContextManager contextManager, IUpgradeLog log, IMigration migration)
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
            var key = _prefix + migrationInfo.Id.ToString();
            properties[key] = JsonConvert.SerializeObject(migrationInfo);

            rootWeb.Update();
            log.Verbose("Storing migration info '{0}'", key);
            clientContext.ExecuteQuery();

            _lastId = id;
        }
    }
}
