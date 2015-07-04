using IonFar.SharePoint.Migration.Services;
using IonFar.SharePoint.Migration.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Represents the configuration of a Synchronizer
    /// </summary>
    public class SynchronizerConfiguration
    {
        private readonly List<ITextFilePreprocessor> _preprocessors = new List<ITextFilePreprocessor>();

        /// <summary>
        /// Creates a new configuration, with default logging to a default TraceUpgrdeLog.
        /// </summary>
        public SynchronizerConfiguration()
        {
            Log = new TraceUpgradeLog();
            HashProvider = new WebPropertyHashProvider();
        }

        /// <summary>
        /// Gets or sets the context manager, which provides to connection to SharePoint.
        /// </summary>
        public IContextManager ContextManager { get; set; }

        /// <summary>
        /// Gets or sets the hash provider, which tracks which file version have previously been uploaded.
        /// </summary>
        public IHashProvider HashProvider { get; set; }

        /// <summary>
        /// Gets or sets which log captures details about the upgrade.
        /// </summary>
        public IUpgradeLog Log { get; set; }

        /// <summary>
        /// Get a collection of components that will pre-process text files before they are uploaded.
        /// </summary>
        public IList<ITextFilePreprocessor> Preprocessors
        {
            get { return _preprocessors; }
        }

        /// <summary>
        /// Ensures all expectations have been met regarding this configuration.
        /// </summary>
        public void Validate()
        {
            if (Log == null) throw new ArgumentException("A log is required to run migrations. Please leave at the default Trace logger, or replace with another logger.");
            if (HashProvider == null) throw new ArgumentException("A hash provider is required. Please leave the default HashProvider, or replace with another.");
            if (ContextManager == null) throw new ArgumentException("A context manager is required. Please leave the default Context Manager, or replace with another.");
        }
    }
}
