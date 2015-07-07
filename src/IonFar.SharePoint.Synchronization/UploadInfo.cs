using System;

namespace IonFar.SharePoint.Synchronization
{
    /// <summary>
    /// Represents a synchronized file
    /// </summary>
    public class UploadInfo
    {
        private readonly string _filePath;
        private readonly byte[] _hashValue;
        private readonly string _serverRelativeUrl;
        private readonly bool _changed;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="filePath">Path of the source file</param>
        /// <param name="serverRelativeUrl">Server relative URL of the destination</param>
        /// <param name="changed">Whether the file was changed or not</param>
        /// <param name="hash">Hash of the file</param>
        public UploadInfo(string filePath, string serverRelativeUrl, bool changed, byte[] hash)
        {
            _filePath = filePath;
            _serverRelativeUrl = serverRelativeUrl;
            _hashValue = new byte[hash.Length];
            Array.Copy(hash, _hashValue, hash.Length);
            _changed = changed;
        }

        /// <summary>
        /// Gets the path of the local file
        /// </summary>
        public string FilePath { get { return _filePath; } }

        /// <summary>
        /// Gets the hash of the file
        /// </summary>
        public byte[] Hash
        {
            get
            {
                // TODO: Consider if this should be copied before returning (non-mutable?)
                return _hashValue;
            }
        }

        /// <summary>
        /// Gets the server relative URL of the file
        /// </summary>
        public string ServerRelativeUrl { get { return _serverRelativeUrl; } }

        /// <summary>
        /// Gets whether the file was changed or not
        /// </summary>
        public bool Changed { get { return _changed; } }
    }
}
