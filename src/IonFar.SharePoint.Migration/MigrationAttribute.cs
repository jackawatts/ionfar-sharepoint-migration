using System;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Used to provide a unique name to a Migration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the migration, used to detect already run migrations
        /// </summary>
        public MigrationAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}