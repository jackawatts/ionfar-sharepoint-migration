using System;
using System.Reflection;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// The value stored to track a <see cref="Migration"/> applied to SharePoint.
    /// </summary>
    public class MigrationInfo
    {
        private readonly DateTimeOffset _appliedAt;
        private readonly long _id;
        private readonly string _name;
        private readonly string _note;

        /// <summary>
        /// Gets the UTC DateTime the migration was applied
        /// </summary>
        public DateTimeOffset AppliedAt { get { return _appliedAt; } }

        /// <summary>
        /// Gets the ID of the migration, as stored in the journal
        /// </summary>
        public long Id { get { return _id; } }

        /// <summary>
        /// Gets the name of the migration, used to detect already run migrations (case insensitive comparison)
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the additional note, stored with the migration
        /// </summary>
        public string Note { get { return _note; } }

        /// <summary>
        /// Creates a new MigrationInfo
        /// </summary>
        /// <param name="id">ID of the migration, as stored in the journal</param>
        /// <param name="name">Name of the migration</param>
        /// <param name="note">Aditional note stored</param>
        /// <param name="appliedAt">UTC DateTime the migration was applied</param>
        [JsonConstructor]
        public MigrationInfo(long id, string name, string note, DateTimeOffset appliedAt)
        {
            _id = id;
            _note = note;
            _name = name;
            _appliedAt = appliedAt;
        }
    }
}