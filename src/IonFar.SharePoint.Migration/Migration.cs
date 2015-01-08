using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Base class for code-based migrations.
    /// </summary>
    public abstract class Migration : IMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        /// <param name="clientContext">The <see cref="ClientContext"/> to which the operations will be applied.</param>
        public abstract void Up(ClientContext clientContext);
    }
}