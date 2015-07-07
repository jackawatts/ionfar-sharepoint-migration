using System;
using System.Collections.Generic;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Represents the result of running a migration
    /// </summary>
    public class MigrationResult
    {
        private readonly List<MigrationInfo> _migrations;
        private readonly bool _successful;
        private readonly Exception _error;

        public MigrationResult(IEnumerable<MigrationInfo> migrations, bool successful, Exception error)
        {
            _migrations = new List<MigrationInfo>();
            _migrations.AddRange(migrations);
            _successful = successful;
            _error = error;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Gets the migrations that were executed.
        /// </summary>
        public IEnumerable<MigrationInfo> Migrations
        {
            get { return _migrations; }
        }

        /// <summary>
        /// Gets a value indicating whether the migration was successful.
        /// </summary>
        public bool Successful
        {
            get { return _successful; }
        }
    }
}
