using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace IonFar.SharePoint.Migration.Providers.Assembly
{
    /// <summary>
    /// Base class for code-based migrations.
    /// </summary>
    public abstract class Migration : IMigration
    {
        /// <summary>
        /// Gets the current folder
        /// </summary>
        protected string BaseFolder
        {
            get
            {
                return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        /// <summary>
        /// Gets the name of the migration, used to detect already run migrations
        /// </summary>
        public string Name
        {
            get
            {
                var migrationType = GetType();
                var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>(inherit: true);
                if (migrationAttribute != null)
                {
                    return migrationAttribute.Name;
                }
                return migrationType.Name;
            }
        }

        /// <summary>
        /// Gets and additional note that is persisted with the migration (optional)
        /// </summary>
        public string Note
        {
            get
            {
                var migrationType = GetType();
                return migrationType.AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// Executes the migration
        /// </summary>
        /// <param name="contextManager">Provides the crrent SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        public abstract void Apply(IContextManager contextManager, IUpgradeLog logger);

    }
}