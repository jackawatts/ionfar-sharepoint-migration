using System;
using System.Reflection;

namespace IonFar.SharePoint.Migration
{
    public interface IMigrator
    {
        void Migrate(Assembly assemblyContainingMigrations);
        void Migrate(Assembly assemblyContainingMigrations, Func<string, bool> filter);
    }
}