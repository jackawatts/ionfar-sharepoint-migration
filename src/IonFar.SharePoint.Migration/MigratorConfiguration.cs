using IonFar.SharePoint.Migration.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Represents the configuration of a Migrator
    /// </summary>
    public class MigratorConfiguration
    {
        private readonly List<IMigrationProvider> _migrationProviders = new List<IMigrationProvider>();

        /// <summary>
        /// Creates a new configuration, with default logging to a default TraceUpgrdeLog.
        /// </summary>
        public MigratorConfiguration()
        {
            Log = new TraceUpgradeLog();
        }

        /// <summary>
        /// Gets or sets which log captures details about the upgrade.
        /// </summary>
        public IUpgradeLog Log { get; set; }

        /// <summary>
        /// Gets a mutable list of migration providers.
        /// </summary>
        public IList<IMigrationProvider> MigrationProviders { get { return _migrationProviders; } }
    }
}
