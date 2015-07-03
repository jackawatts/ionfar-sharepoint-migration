using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    /// <summary>
    /// Provides PowerShell script migrations contained in a folder.
    /// </summary>
    public class ScriptMigrationProvider : IMigrationProvider
    {
        string _folderPath;

        public ScriptMigrationProvider(string folderPath)
        {
            if (folderPath == null) { throw new ArgumentNullException("folderPath"); }
            _folderPath = folderPath;
        }

        public IEnumerable<IMigration> GetMigrations(IContextManager contextManager, IUpgradeLog log)
        {
            var filePaths = System.IO.Directory.EnumerateFiles(_folderPath);

            //Console.WriteLine("GetMigrations " + _folderPath);

            foreach (var filePath in filePaths)
            {
                if (System.IO.Path.GetExtension(filePath).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    var migration = new ScriptMigration(filePath);
                    yield return migration;
                }
            }
        }

        class ScriptMigration : IMigration
        {
            string _filePath;

            public ScriptMigration(string filePath)
            {
                _filePath = filePath;
            }

            public string Name
            {
                get
                {
                    var fileName = System.IO.Path.GetFileName(_filePath);
                    return fileName;
                }
            }

            public string Note
            {
                get
                {
                    return _filePath;
                }
            }

            public void Apply(IContextManager contextManager, IUpgradeLog logger)
            {
                throw new NotImplementedException();
            }
        }
    }
}
