using System;

namespace IonFar.SharePoint.Migration.Providers
{
    /// <summary>
    /// A log that writes to the console in a colorful way.
    /// </summary>
    public class ConsoleUpgradeLog : IUpgradeLog
    {
        /// <summary>
        /// Create a console logger, that by default does not output verbose messages.
        /// </summary>
        public ConsoleUpgradeLog()
        {
        }

        /// <summary>
        /// Create a console logger, specifying whether to output verbose messages. 
        /// </summary>
        /// <param name="verbose">true to output verbose messages; false (default) to not output</param>
        public ConsoleUpgradeLog(bool verbose)
        {
            IncludeVerbose = verbose;
        }

        /// <summary>
        /// Gets or sets whether to include verbose messages in the output.
        /// </summary>
        public bool IncludeVerbose { get; set; }
 
        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Error(string format, params object[] args)
        {
            Write(ConsoleColor.Yellow, format, args);
        }

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Information(string format, params object[] args)
        {
            Write(ConsoleColor.White, format, args);
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Warning(string format, params object[] args)
        {
            Write(ConsoleColor.Red, format, args);
        }

        /// <summary>
        /// Writes a verbose message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Verbose(string format, params object[] args)
        {
            Write(ConsoleColor.DarkCyan, format, args);
        }

        private static void Write(ConsoleColor color, string format, object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }
    }
}
