using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    /// <summary>
    /// Provides thes migrations contained in an assembly.
    /// </summary>
    /// <remarks>Migrations must be decorated with a <see cref="MigrationAttribute"/></remarks>
    public class AssemblyMigrationProvider : IMigrationProvider
    {
        Assembly _assemblyContainingMigrations;
        Func<string, bool> _filter;

        /// <summary>
        /// Creates a provider for all IMigrations in the assembly.
        /// </summary>
        /// <param name="assemblyContainingMigrations">The assembly containing the migrations to run.</param>
        public AssemblyMigrationProvider(Assembly assemblyContainingMigrations)
            : this(assemblyContainingMigrations, name => true)
        {
        }

        /// <summary>
        /// Creates a provider for all IMigrations in the assembly, satisfying a particular filter.
        /// </summary>
        /// <param name="assemblyContainingMigrations">The assembly containing the migrations to run.</param>
        /// <param name="filter">A function that accepts a Type.FullName and returns true if the given type is to be migrated.</param>
        public AssemblyMigrationProvider(Assembly assemblyContainingMigrations, Func<string, bool> filter)
        {
            _assemblyContainingMigrations = assemblyContainingMigrations;
            _filter = filter;
        }

        /// <summary>
        /// Gets all migrations that should be executed, ordered by name.
        /// </summary>
        public IEnumerable<IMigration> GetMigrations(IContextManager contextManager, IUpgradeLog log)
        {
            var availableMigrations = _assemblyContainingMigrations
                .GetExportedTypes()
                .Where(candidateType => typeof(IMigration).IsAssignableFrom(candidateType) &&
                    !candidateType.IsAbstract &&
                    candidateType.IsClass &&
                    candidateType.GetConstructor(Type.EmptyTypes) != null)
                .Where(migrationType => _filter(migrationType.FullName))
                .Select(migrationType => (IMigration)Activator.CreateInstance(migrationType))
                .OrderBy(migration => migration.Name)
                .ToList();

            return availableMigrations;
        }
    }
}
