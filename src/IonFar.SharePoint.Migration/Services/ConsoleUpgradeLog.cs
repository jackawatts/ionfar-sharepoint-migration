using System;

namespace IonFar.SharePoint.Migration.Services
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
        /// Writes a critical error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Critical(string format, params object[] args)
        {
            WriteLine(ConsoleColor.Red, "CRITICAL: " + format, args);
        }

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Error(string format, params object[] args)
        {
            WriteLine(ConsoleColor.DarkRed, "ERROR: " + format, args);
        }

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Information(string format, params object[] args)
        {
            WriteLine(ConsoleColor.Gray, format, args);
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Warning(string format, params object[] args)
        {
            WriteLine(ConsoleColor.Yellow, "WARN: " + format, args);
        }

        /// <summary>
        /// Writes a verbose message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Verbose(string format, params object[] args)
        {
            if (IncludeVerbose)
            {
                WriteLine(ConsoleColor.DarkCyan, format, args);
            }
        }

        ///// <summary>
        ///// Writes directly to the log (no line break).
        ///// </summary>
        ///// <param name="format">The format.</param>
        ///// <param name="args">The args.</param>
        //public void Write(string format, params object[] args)
        //{
        //    Console.Write(format, args);
        //}

        private static void WriteLine(ConsoleColor color, string format, object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }
    }
}
