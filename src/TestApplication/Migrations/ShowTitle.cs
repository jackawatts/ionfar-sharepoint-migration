using System;
using IonFar.SharePoint.Migration;
using Microsoft.SharePoint.Client;

namespace TestApplication.Migrations
{
    [Migration(10001, true)]
    public class ShowTitle : IMigration
    {
        public void Up(ClientContext clientContext, ILogger logger)
        {
            clientContext.Load(clientContext.Web, w => w.Title);
            clientContext.ExecuteQuery();

            Console.WriteLine("Your site title is: " + clientContext.Web.Title);
        }
    }
}
