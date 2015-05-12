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
            //var availableMigrationsIds = availableMigrations.Select(am => am.Id).ToList();

            var clientContext = contextManager.CurrentContext;

            var rootWeb = clientContext.Site.RootWeb;

            clientContext.Load(rootWeb);

            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);

            clientContext.ExecuteQuery();

            var appliedMigrations = properties.FieldValues.Where(f => f.Key.StartsWith(_prefix));

            foreach (var migration in appliedMigrations)
            {
                MigrationInfo migrationInfo = null;
                try
                {
                    migrationInfo = JsonConvert.DeserializeObject(migration.Value.ToString(), typeof(MigrationInfo)) as MigrationInfo;
                }
                catch (Exception ex)
                {
                    log.Warning("Error deserializing migration '{0}'. Exception: {1}", migration.Key, ex);
                }
                if (migrationInfo != null)
                {
                    yield return migrationInfo;
                }
            }
        }

        /// <summary>
        /// Records a migration as having been run.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        /// <param name="migration">Migration that has been run</param>
        public void StoreExecutedMigration(IContextManager contextManager, IUpgradeLog log, MigrationInfo migrationInfo)
        {
            var clientContext = contextManager.CurrentContext;

            var rootWeb = clientContext.Site.RootWeb;

            clientContext.Load(rootWeb);

            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);
            clientContext.ExecuteQuery();

            var id = _prefix + migrationInfo.Version;
            properties[id] = JsonConvert.SerializeObject(migrationInfo);

            rootWeb.Update();
            clientContext.ExecuteQuery();
        }
    }
}
