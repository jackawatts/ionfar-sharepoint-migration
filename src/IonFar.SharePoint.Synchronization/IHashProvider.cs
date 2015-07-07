using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization
{
    /// <summary>
    /// Provides hash values for comparison before uploading files; use NullHashProvider if not required.
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Gets the hash for the specified file.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="serverRelativeUrl">Server relative URL of the file; some implementations may allow prefixed URLs.</param>
        /// <returns>The stored hash, or an empty array if there is no stored hash</returns>
        byte[] GetFileHash(IContextManager contextManager, IUpgradeLog logger, string serverRelativeUrl);

        /// <summary>
        /// Stores the hash for the specified file.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="serverRelativeUrl">Server relative URL of the file; some implementations may allow prefixed URLs.</param>
        /// <param name="hash">Hash value to store</param>
        void StoreFileHash(IContextManager contextManager, IUpgradeLog logger, string serverRelativeUrl, byte[] hash);
    }
}
