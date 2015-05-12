using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Output
{
    /// <summary>
    /// A log that writes to System.Diagnostics.
    /// </summary>
    public class TraceUpgradeLog : IUpgradeLog
    {
        const string DefaultSourceName = "IonFar.SharePoint";
        TraceSource _source;

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
    }
}
