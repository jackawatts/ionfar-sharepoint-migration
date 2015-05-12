using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Base class for code-based migrations.
    /// </summary>
    public abstract class Migration : IMigration
    {
        public abstract void Up(ClientContext clientContext, IUpgradeLog logger);

        protected string BaseFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }
    }
}