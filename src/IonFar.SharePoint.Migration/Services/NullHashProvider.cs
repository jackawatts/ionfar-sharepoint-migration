using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// A hash provider that does not store anything, and always returns an empty hash.
    /// </summary>
    public class NullHashProvider : IHashProvider
    {
        /// <summary>
        /// Gets the hash for the specified file; alwasy returns the empty hash.
        /// </summary>
        /// <param name="serverRelativeUrl">Ignored</param>
        /// <returns>An empty array</returns>
        public byte[] GetFileHash(string fileUrl)
        {
            return new byte[0];
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="serverRelativeUrl">Ignored</param>
        /// <param name="hash">Ignored</param>
        public void StoreFileHash(string fileUrl, byte[] localHash)
        {
        }
    }
}
