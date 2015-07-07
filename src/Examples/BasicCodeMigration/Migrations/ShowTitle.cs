using System;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;

namespace BasicCodeMigration.Migrations
{
    [Migration("Code0001")]
    public class ShowTitle : Migration
    {
        public override void Apply(IContextManager contextManager, IUpgradeLog logger)
        {
            logger.Information("Running migration for URL: {0}", contextManager.CurrentContext.Url);

            var clientContext = contextManager.CurrentContext;

            clientContext.Load(clientContext.Web, w => w.Title);
            clientContext.ExecuteQuery();

            logger.Warning("Site title is: {0}", clientContext.Web.Title);
        }
    }
}
