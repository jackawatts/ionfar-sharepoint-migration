using System;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Used to provide a unique version number to an IMigration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(long version,bool overrideCurrentDeployment = false)
        {
            if (version < 0) throw new ArgumentOutOfRangeException("version", "Migration version must be greater than zero.");
            Version = version;
            OverrideCurrentDeployment = overrideCurrentDeployment;
        }

        public bool OverrideCurrentDeployment { get; private set; }
        public long Version { get; private set; }
    }
}