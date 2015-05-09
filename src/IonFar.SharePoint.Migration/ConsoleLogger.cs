using System;

namespace IonFar.SharePoint.Migration
{
    public class ConsoleLogger : ILogger
    {
        public void Information(string message, params object[] args)
        {
            Console.WriteLine(message);
        }

        public void Warning(string message, params object[] args)
        {
            Console.WriteLine(message);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(message);
        }
    }
}
