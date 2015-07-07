using System;
using System.Collections.Generic;

namespace IonFar.SharePoint.Migration.Providers.Script
{
    /// <summary>
    /// Provides PowerShell script migrations contained in a folder.
    /// </summary>
    public class ScriptMigrationProvider : IMigrationProvider
    {
        string _folderPath;
        private readonly Dictionary<string, object> _variables;

        public ScriptMigrationProvider(string folderPath)
            : this(folderPath, new Dictionary<string, object>())
        {
        }

        public ScriptMigrationProvider(string folderPath, IDictionary<string, object> variables)
        {
            if (folderPath == null) { throw new ArgumentNullException("folderPath"); }
            _folderPath = folderPath;
            _variables = new Dictionary<string, object>(variables);
        }

        public IDictionary<string, object> Variables
        {
            get { return _variables; }
        }

        public IEnumerable<IMigration> GetMigrations(IContextManager contextManager, IUpgradeLog log)
        {
            var filePaths = System.IO.Directory.EnumerateFiles(_folderPath);

            //Console.WriteLine("GetMigrations " + _folderPath);

            foreach (var filePath in filePaths)
            {
                if (System.IO.Path.GetExtension(filePath).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    var migration = new ScriptMigration(filePath, _variables);
                    yield return migration;
                }
            }
        }

    }
}
