using System;
using System.Reflection;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// The value stored in the Property Bag to track a <see cref="Migration"/> applied to SharePoint.
    /// </summary>
    public class MigrationInfo
    {
        /// <summary>
        /// Gets the UTC DateTime the migration was applied
        /// </summary>
        public DateTimeOffset AppliedAt { get; private set; }

        /// <summary>
        /// Gets the ID of the migration, as stored in the journal
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Gets the name of the migration, used to detect already run migrations (case insensitive comparison)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the additional note, stored with the migration
        /// </summary>
        public string Note { get; private set; }

        [JsonConstructor]
        public MigrationInfo(long id, string name, string note, DateTimeOffset appliedAt)
        {
            Id = id;
            Note = note;
            Name = name;
            AppliedAt = appliedAt;
        }

        //public void ApplyMigration(IContextManager contextManager, IUpgradeLog logger)
        //{
        //    var migrationtype = Type.GetType(Note);
        //    var migration = (IMigration)Activator.CreateInstance(migrationtype);

        //    migration.Apply(contextManager.CurrentContext, logger);

        //    AppliedAtUtc = DateTime.UtcNow;
        //}
    }
}