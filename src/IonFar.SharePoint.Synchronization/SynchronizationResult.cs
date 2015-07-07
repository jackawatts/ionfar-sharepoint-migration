using System;
using System.Collections.Generic;

namespace IonFar.SharePoint.Synchronization
{
    /// <summary>
    /// Represents the result of running a synchronisation
    /// </summary>
    public class SynchronizationResult
    {
        private readonly List<UploadInfo> _files;
        private readonly bool _successful;
        private readonly Exception _error;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public SynchronizationResult(IEnumerable<UploadInfo> files, bool successful, Exception error)
        {
            _files = new List<UploadInfo>();
            _files.AddRange(files);
            _successful = successful;
            _error = error;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Gets the files that were synchronised.
        /// </summary>
        public IEnumerable<UploadInfo> Files
        {
            get { return _files; }
        }

        /// <summary>
        /// Gets a value indicating whether the migration was successful.
        /// </summary>
        public bool Successful
        {
            get { return _successful; }
        }
    }
}
