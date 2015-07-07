using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization.Providers
{
    /// <summary>
    /// A hash provider that does not store anything, and always returns an empty hash.
    /// </summary>
    public class NullHashProvider : IHashProvider
    {
        /// <summary>
        /// Gets the hash for the specified file; alwasy returns the empty hash.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="serverRelativeUrl">Ignored</param>
        /// <returns>An empty array</returns>
        public byte[] GetFileHash(IContextManager contextManager, IUpgradeLog logger, string serverRelativeUrl)
        {
            return new byte[0];
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="serverRelativeUrl">Ignored</param>
        /// <param name="localHash">Ignored</param>
        public void StoreFileHash(IContextManager contextManager, IUpgradeLog logger, string serverRelativeUrl, byte[] localHash)
        {
        }
    }
}
