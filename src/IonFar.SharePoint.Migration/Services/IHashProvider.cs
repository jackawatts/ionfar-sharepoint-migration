using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// Provides hash values for comparison before uploading files; use NullHashProvider if not required.
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Gets the hash for the specified file.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file; some implementations may allow prefixed URLs.</param>
        /// <returns>The stored hash, or an empty array if there is no stored hash</returns>
        byte[] GetFileHash(string serverRelativeUrl);

        /// <summary>
        /// Stores the hash for the specified file.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file; some implementations may allow prefixed URLs.</param>
        /// <param name="hash">Hash value to store</param>
        void StoreFileHash(string serverRelativeUrl, byte[] hash);
    }
}
