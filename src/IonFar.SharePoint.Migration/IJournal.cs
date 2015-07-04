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
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        IEnumerable<MigrationInfo> GetExecutedMigrations(IContextManager contextManager, IUpgradeLog logger);

        /// <summary>
        /// Records a migration as having been run.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="migration">Migration that has been run</param>
        MigrationInfo StoreExecutedMigration(IContextManager contextManager, IUpgradeLog logger, IMigration migration);
    }
}
