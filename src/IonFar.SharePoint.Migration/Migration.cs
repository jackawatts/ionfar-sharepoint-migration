using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Base class for code-based migrations.
    /// </summary>
    public abstract class Migration : IMigration
    {
        public abstract void Up(ClientContext clientContext, ILogger logger);

        public virtual void Down(ClientContext clientContext, ILogger logger) { }

        protected string BaseFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }
    }
}