using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration
{
    public interface IMigration
    {
        /// <summary>
        /// Gets the name of the migration, used to detect already run migrations (case insensitive comparison)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets and additional note that is persisted with the migration (may be nothing)
        /// </summary>
        string Note { get; }

        /// <summary>
        /// Executes the migration
        /// </summary>
        /// <param name="contextManager">Provides the crrent SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        void Apply(IContextManager contextManager, IUpgradeLog logger);
    }
}