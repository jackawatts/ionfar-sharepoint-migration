using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration.Providers
{
    /// <summary>
    /// Enables multiple executions of idempotent migrations.
    /// </summary>
    public class NullJournal : IJournal
    {
        /// <summary>
        /// Always returns an empty array.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        public IEnumerable<MigrationInfo> GetExecutedMigrations(ClientContext clientContext)
        {
            return new MigrationInfo[0];
        }

        /// <summary>
        /// Simply returns, and does not store the migration
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        /// <param name="migration">Migration that has been run</param>
        public void StoreExecutedMigration(ClientContext clientContext, MigrationInfo migrationInfo)
        {
        }
    }
}
