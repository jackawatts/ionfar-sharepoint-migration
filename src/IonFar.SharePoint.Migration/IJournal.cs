using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Interface that allows different projects to store version information differently.
    /// </summary>
    public interface IJournal
    {
        /// <summary>
        /// Gets the migrations that have already been executed.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        IEnumerable<MigrationInfo> GetExecutedMigrations(ClientContext clientContext);

        /// <summary>
        /// Records a migration as having been run.
        /// </summary>
        /// <param name="clientContext">Context to the SharePoint server</param>
        /// <param name="migration">Migration that has been run</param>
        void StoreExecutedMigration(ClientContext clientContext, MigrationInfo migrationInfo);
    }
}
