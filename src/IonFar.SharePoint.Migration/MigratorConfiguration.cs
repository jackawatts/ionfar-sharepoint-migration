using IonFar.SharePoint.Migration.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    public class MigratorConfiguration
    {
        public MigratorConfiguration()
        {
            Log = new TraceUpgradeLog();
        }

        public IUpgradeLog Log { get; set; }
    }
}
