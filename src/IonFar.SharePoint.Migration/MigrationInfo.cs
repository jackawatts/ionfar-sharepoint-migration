using System;
using System.Reflection;
using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// The document stored in RavenDB to track a <see cref="Migration"/> applied to the database.
    /// </summary>
    public class MigrationInfo
    {
        public const string Prefix = "migrationinfos/";

        /// <summary>
        /// The type containing the Migration to be applied.
        /// </summary>
        public Type MigrationType { get; private set; }
        /// <summary>
        /// The Id property of this instance.
        /// </summary>
        public string Id { get; protected set; }
        /// <summary>
        /// Get the Version of <see cref="MigrationAttribute"/> associated with the MigrationType.
        /// </summary>
        public long Version { get; protected set; }
        /// <summary>
        /// Gets the FullName of the MigrationType.
        /// </summary>
        public string FullName { get; protected set; }
        /// <summary>
        /// Gets the UTC DateTime the migration was applied.
        /// </summary>
        public DateTime AppliedAtUtc { get; set; }

        protected MigrationInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationInfo"/> class with a type containing a migration.
        /// </summary>
        /// <param name="migrationType">The type containing the migration.</param>
        public MigrationInfo(Type migrationType)
        {
            MigrationType = migrationType;
            FullName = migrationType.FullName;
            var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>(inherit: true);
            if (migrationAttribute == null)
            {
                throw new InvalidOperationException(string.Format("The migration class {0} is missing its {1}.", migrationType.FullName, typeof(MigrationAttribute)));
            }

            Version = migrationAttribute.Version;
            Id = Prefix + Version;
        }

        /// <summary>
        /// Applies the MigrationType to the given <see cref="ClientContext"/>.
        /// </summary>
        /// <param name="clientContext">The <see cref="ClientContext"/> to apply the MigrationType to.</param>
        public void ApplyMigration(ClientContext clientContext)
        {
            var migration = (IMigration)Activator.CreateInstance(MigrationType);
            migration.Up(clientContext);
            AppliedAtUtc = DateTime.UtcNow;
        }
    }
}