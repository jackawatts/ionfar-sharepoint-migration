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
        //public const string DefaultPrefix = "migrationinfos";

        //string _prefix;

        //public PropertyBagJournal()
        //    : this(DefaultPrefix)
        //{
        //}

        //public PropertyBagJournal(string prefix)
        //{
        //    _prefix = prefix;
        //}


        /// <summary>
        /// Gets the migrations that have already been executed.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        public IEnumerable<MigrationInfo> GetExecutedMigrations(ClientContext clientContext)
        {
            //var availableMigrationsIds = availableMigrations.Select(am => am.Id).ToList();

            var rootWeb = clientContext.Site.RootWeb;

            clientContext.Load(rootWeb);

            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);

            clientContext.ExecuteQuery();

            var appliedMigrations = properties.FieldValues.Where(f => f.Key.StartsWith(MigrationInfo.Prefix));

            foreach (var migration in appliedMigrations)
            {
                MigrationInfo migrationInfo = null;
                try
                {
                    migrationInfo = JsonConvert.DeserializeObject(migration.Value.ToString(), typeof(MigrationInfo)) as MigrationInfo;
                }
                catch (Exception ex)
                {
                    // Ignore ? (should probably log)
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
        public void StoreExecutedMigration(ClientContext clientContext, MigrationInfo migrationInfo)
        {
            var rootWeb = clientContext.Site.RootWeb;

            clientContext.Load(rootWeb);

            var properties = rootWeb.AllProperties;
            clientContext.Load(properties);
            clientContext.ExecuteQuery();

            properties[migrationInfo.Id] = JsonConvert.SerializeObject(migrationInfo);

            rootWeb.Update();
            clientContext.ExecuteQuery();
        }
    }
}
