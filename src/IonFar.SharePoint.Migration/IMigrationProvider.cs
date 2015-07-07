using System.Collections.Generic;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Provides migrations to be executed.
    /// </summary>
    public interface IMigrationProvider
    {
        /// <summary>
        /// Gets all migrations that should be executed, in the order they should be executed.
        /// </summary>
        IEnumerable<IMigration> GetMigrations(IContextManager contextManager, IUpgradeLog log);
    }
}
