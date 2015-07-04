using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Sync
{
    /// <summary>
    /// A hash provider that stores uploaded file hashs in a property bag of the web.
    /// </summary>
    public class WebPropertyHashProvider : IHashProvider
    {
        private const string DefaultPropertyBagKey = "ION_UploadHash";

        private static HashAlgorithm DefaultHashAlgorithm = SHA256.Create();

        HashAlgorithm _hashAlgorithm;
        Dictionary<string, string> _hashValues;
        string _propertyBagKey;

        /// <summary>
        /// Creates a hash provider for the specified web, with the default hash algorithm (SHA256) and property bag key.
        /// </summary>
        public WebPropertyHashProvider()
            : this(null, null)
        {
        }

        /// <summary>
        /// Creates a hash provider for the specified web, with the specified hash algorithm and property bag key.
        /// </summary>
        /// <param name="propertyBagKey">Key to use to store the property; if null the default (ION_UploadHash) is used</param>
        /// <param name="hashAlgorithm">Hash algoritm to use; if null the default (SHA256) is used</param>
        public WebPropertyHashProvider(string propertyBagKey, HashAlgorithm hashAlgorithm)
        {
            _propertyBagKey = propertyBagKey ?? DefaultPropertyBagKey;
            _hashAlgorithm = hashAlgorithm ?? DefaultHashAlgorithm;

        }

        /// <summary>
        /// Gets the hash for the specified file.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="prefixedServerRelativeUrl">Server relative URL of the file; a prefixed URL is allowed.</param>
        /// <returns>The stored hash, or an empty array if there is no stored hash</returns>
        public byte[] GetFileHash(IContextManager contextManager, IUpgradeLog logger, string prefixedServerRelativeUrl)
        {
            if (_hashValues == null)
            {
                InitHashValues(contextManager, logger);
            }

            var clientContext = contextManager.CurrentContext;
            var serverRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(clientContext.Site, clientContext.Web, prefixedServerRelativeUrl);

            var webRelativeUrl = serverRelativeUrl.Substring(clientContext.Web.ServerRelativeUrl.Length);
            if (webRelativeUrl.StartsWith("/"))
            {
                webRelativeUrl = webRelativeUrl.Substring(1);
            }
            var hash = new List<byte>();
            var hashString = "";
            if (_hashValues.TryGetValue(webRelativeUrl, out hashString))
            {
                var parts = hashString.Split('-');
                foreach (var part in parts)
                {
                    hash.Add(byte.Parse(part, System.Globalization.NumberStyles.HexNumber));
                }
            }
            return hash.ToArray();
        }

        /// <summary>
        /// Stores the hash for the specified file.
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="prefixedServerRelativeUrl">Server relative URL of the file; a prefixed URL is allowed</param>
        /// <param name="hash">Hash value to store</param>
        public void StoreFileHash(IContextManager contextManager, IUpgradeLog logger, string prefixedServerRelativeUrl, byte[] hash)
        {
            if (_hashValues == null)
            {
                throw new InvalidOperationException("Must have been initialized first (by calling GetFileHash)");
            }

            var clientContext = contextManager.CurrentContext;
            var serverRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(clientContext.Site, clientContext.Web, prefixedServerRelativeUrl);

            var webRelativeUrl = serverRelativeUrl.Substring(clientContext.Web.ServerRelativeUrl.Length);
            if (webRelativeUrl.StartsWith("/"))
            {
                webRelativeUrl = webRelativeUrl.Substring(1);
            }
            var hashString = BitConverter.ToString(hash);
            _hashValues[webRelativeUrl] = hashString;
            var propertyValue = JsonConvert.SerializeObject(_hashValues.ToList());
            clientContext.Web.AllProperties[_propertyBagKey] = propertyValue;
            clientContext.Web.Update();
            clientContext.ExecuteQuery();
        }

        private void InitHashValues(IContextManager contextManager, IUpgradeLog logger)
        {
            var clientContext = contextManager.CurrentContext;
            var web = clientContext.Web;
            clientContext.Load(web, w => w.ServerRelativeUrl, w => w.AllProperties);
            clientContext.ExecuteQuery();

            object value = null;
            if (web.AllProperties.FieldValues.TryGetValue(_propertyBagKey, out value))
            {
                //Console.WriteLine("JSON: {0}", value);
                var list = JsonConvert.DeserializeObject<IList<KeyValuePair<string, string>>>((string)value);
                _hashValues = list.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                _hashValues = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            }
        }

    }
}
