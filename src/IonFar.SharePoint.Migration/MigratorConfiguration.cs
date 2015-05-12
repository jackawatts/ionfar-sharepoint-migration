using IonFar.SharePoint.Migration.Providers;
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
            Journal = new PropertyBagJournal();
        }

        /// <summary>
        /// Gets or sets the journal, which tracks the migrations that have already been run.
        /// </summary>
        public IJournal Journal { get; set; }

        /// <summary>
        /// Gets or sets which log captures details about the upgrade.
        /// </summary>
        public IUpgradeLog Log { get; set; }

        /// <summary>
        /// Gets a mutable list of migration providers.
        /// </summary>
        public IList<IMigrationProvider> MigrationProviders { get { return _migrationProviders; } }

        /// <summary>
        /// Ensures all expectations have been met regarding this configuration.
        /// </summary>
        public void Validate()
        {
            if (Log == null) throw new ArgumentException("A log is required to run migrations. Please leave at the default Trace logger, or replace with another logger.");
            if (Journal == null) throw new ArgumentException("A journal is required. Please leave the default Journal, or replace with another.");
            if (MigrationProviders.Count == 0) throw new ArgumentException("No migration providers were added. Please add an assembly (or other) migration provider.");
            //if (ConnectionManager == null) throw new ArgumentException("The ConnectionManager is null. What do you expect to upgrade?");
        }
    }
}
