using System.Diagnostics;

namespace IonFar.SharePoint.Migration.Logs
{
    /// <summary>
    /// A log that writes to System.Diagnostics.
    /// </summary>
    public class TraceUpgradeLog : IUpgradeLog
    {
        private const string DefaultSourceName = "IonFar.SharePoint";
        private readonly TraceSource _source;

        /// <summary>
        /// Creates a logger to the default source "IonFar.SharePoint"
        /// </summary>
        public TraceUpgradeLog()
            : this(DefaultSourceName)
        {
        }

        /// <summary>
        /// Creates a logger to the named trace source
        /// </summary>
        /// <param name="sourceName"></param>
        public TraceUpgradeLog(string sourceName)
        {
            _source = new TraceSource(sourceName);
        }

        /// <summary>
        /// Writes a critical error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Critical(string format, params object[] args)
        {
            _source.TraceEvent(TraceEventType.Critical, 0, format, args);
        }

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Error(string format, params object[] args)
        {
            _source.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Information(string format, params object[] args)
        {
            _source.TraceEvent(TraceEventType.Information, 0, format, args);
        }

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Verbose(string format, params object[] args)
        {
            _source.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

        /// <summary>
        /// Writes a verbose message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void Warning(string format, params object[] args)
        {
            _source.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        ///// <summary>
        ///// Writes directly to the log (no line break).
        ///// </summary>
        ///// <param name="format">The format.</param>
        ///// <param name="args">The args.</param>
        //public void Write(string format, params object[] args)
        //{
        //    Trace.Write(string.Format(format, args));
        //}
    }
}
