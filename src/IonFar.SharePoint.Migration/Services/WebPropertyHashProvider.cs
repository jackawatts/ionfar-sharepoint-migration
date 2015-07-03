using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
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
        Site _site;
        Web _web;

        /// <summary>
        /// Creates a hash provider for the specified web, with the default hash algorithm (SHA256) and property bag key.
        /// </summary>
        /// <param name="web">The Web to store the property in</param>
        /// <remarks>
        /// <para>
        /// Note that because the Site is not provided, URLs with the prefix "~sitecollection/" must be resolved before passing to the hash provider.
        /// </para>
        /// </remarks>
        public WebPropertyHashProvider(Site site, Web web)
            : this(site, web, null, null)
        {
        }

        /// <summary>
        /// Creates a hash provider for the specified web, with the specified hash algorithm and property bag key.
        /// </summary>
        /// <param name="site">Provide the Site to allow the component to resolve URLs with the prefix "~sitecollection/".</param>
        /// <param name="web">The Web to store the property in</param>
        /// <param name="hashAlgorithm">Hash algoritm to use; if null the default (SHA256) is used</param>
        /// <param name="propertyBagKey">Key to use to store the property; if null the default (ION_UploadHash) is used</param>
        public WebPropertyHashProvider(Site site, Web web, HashAlgorithm hashAlgorithm, string propertyBagKey)
        {
            if (web == null) { throw new NullReferenceException("web"); }

            _site = site;
            _web = web;
            _hashAlgorithm = hashAlgorithm ?? DefaultHashAlgorithm;
            _propertyBagKey = propertyBagKey ?? DefaultPropertyBagKey;

            _web.Context.Load(_web, w => w.ServerRelativeUrl, w => w.AllProperties);
            _web.Context.ExecuteQuery();

            object value = null;
            if (_web.AllProperties.FieldValues.TryGetValue(_propertyBagKey, out value))
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

        /// <summary>
        /// Gets the hash for the specified file.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file; a prefixed URL is allows, but "~sitecollection/" can only be used if the Site was provided.</param>
        /// <returns>The stored hash, or an empty array if there is no stored hash</returns>
        public byte[] GetFileHash(string fileUrl)
        {
            fileUrl = SPUrlUtility.ResolveServerRelativeUrl(_site, _web, fileUrl);

            var webRelativeUrl = fileUrl.Substring(_web.ServerRelativeUrl.Length);
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
        /// <param name="serverRelativeUrl">Server relative URL of the file; a prefixed URL is allows, but "~sitecollection/" can only be used if the Site was provided.</param>
        /// <param name="hash">Hash value to store</param>
        public void StoreFileHash(string fileUrl, byte[] hash)
        {
            fileUrl = SPUrlUtility.ResolveServerRelativeUrl(_site, _web, fileUrl);

            var webRelativeUrl = fileUrl.Substring(_web.ServerRelativeUrl.Length);
            if (webRelativeUrl.StartsWith("/"))
            {
                webRelativeUrl = webRelativeUrl.Substring(1);
            }
            var hashString = BitConverter.ToString(hash);
            _hashValues[webRelativeUrl] = hashString;
            var propertyValue = JsonConvert.SerializeObject(_hashValues.ToList());
            _web.AllProperties[_propertyBagKey] = propertyValue;
            _web.Update();
            _web.Context.ExecuteQuery();
        }
    }
}
