using System;
using System.Collections.Generic;

namespace IonFar.SharePoint.Migration.Journals
{
    /// <summary>
    /// Enables multiple executions of idempotent migrations.
    /// </summary>
    public class NullJournal : IJournal
    {
        /// <summary>
        /// Always returns an empty array.
        /// </summary>
        /// <param name="contextManager">The ContextManager.</param>
        /// <param name="log">The UpgradeLog.</param>
        public IEnumerable<MigrationInfo> GetExecutedMigrations(IContextManager contextManager, IUpgradeLog log)
        {
            return new MigrationInfo[0];
        }

        /// <summary>
        /// Simply returns, and does not store the migration
        /// </summary>
        /// <param name="contextManager">The contentManager.</param>
        /// <param name="log">The UpgradeLog.</param>
        /// <param name="migration">Migration that has been run</param>
        public MigrationInfo StoreExecutedMigration(IContextManager contextManager, IUpgradeLog log, IMigration migration)
        {
            return new MigrationInfo(0, migration.Name, migration.Note, DateTimeOffset.UtcNow);
        }
    }
}
