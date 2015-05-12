using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Provides migrations to be executed.
    /// </summary>
    public interface IMigrationProvider
    {
        /// <summary>
        /// Gets all migrations that should be executed.
        /// </summary>
        IEnumerable<MigrationInfo> GetMigrations();
    }
}
