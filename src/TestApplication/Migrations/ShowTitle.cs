using System;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;

namespace TestApplication.Migrations
{
    [Migration("Test0001")]
    public class ShowTitle : Migration
    {
        public override void Apply(IContextManager contextManager, IUpgradeLog logger)
        {
            var clientContext = contextManager.CurrentContext;

            clientContext.Load(clientContext.Web, w => w.Title);
            clientContext.ExecuteQuery();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Your site title is: " + clientContext.Web.Title);
            Console.ResetColor();
        }
    }
}
