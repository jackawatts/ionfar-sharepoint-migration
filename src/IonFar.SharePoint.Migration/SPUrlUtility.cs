using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Static methods to modify URL paths.
    /// </summary>
    public static class SPUrlUtility
    {
        private static readonly char[] InvalidUrlChars = new char[]
            {
                '\\',
                '~',
                '#',
                '%',
                '&',
                '*',
                '{',
                '}',
                '/',
                ':',
                '<',
                '>',
                '?',
                '+',
                '|',
                '"',
                '\0',
                '\u0001',
                '\u0002',
                '\u0003',
                '\u0004',
                '\u0005',
                '\u0006',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r',
                '\u000e',
                '\u000f',
                '\u0010',
                '\u0011',
                '\u0012',
                '\u0013',
                '\u0014',
                '\u0015',
                '\u0016',
                '\u0017',
                '\u0018',
                '\u0019',
                '\u001a',
                '\u001b',
                '\u001c',
                '\u001d',
                '\u001e',
                '\u001f'
            };

        /// <summary>
        /// Alternate character used to separate URL path levels
        /// </summary>
        public static readonly char AltUrlSeparatorChar = '\\';

        /// <summary>
        /// Character used to separate URL path levels
        /// </summary>
        public static readonly char UrlSeparatorChar = '/';


        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="paths">The path elements to combine</param>
        /// <returns>The combined path</returns>
        public static string Combine(params string[] paths)
        {
            if (paths == null) { throw new ArgumentNullException("paths"); }

            if (paths.Length == 0)
            {
                return string.Empty;
            }

            var pathBuilder = paths[0];
            for (var index = 1; index < paths.Length; index++)
            {
                pathBuilder = Combine(pathBuilder, paths[index]);
            }
            return pathBuilder;
        }

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path1">The first part of the path</param>
        /// <param name="path2">The second part of the path</param>
        /// <returns>The combined path</returns>
        public static string Combine(string path1, string path2)
        {
            if (path1 == null) { throw new ArgumentNullException("path1"); }
            if (path2 == null) { throw new ArgumentNullException("path2"); }

            if (path2.Length == 0)
            {
                return path1;
            }
            if (path1.Length == 0)
            {
                return path2;
            }
            string combined = path1;
            var last = path1[path1.Length - 1];
            if (last != SPUrlUtility.UrlSeparatorChar && last != SPUrlUtility.AltUrlSeparatorChar)
            {
                combined += "/";
            }
            combined += path2;

            return combined;
        }

        /// <summary>
        /// Resolves site collection and site relative URLs.
        /// </summary>
        /// <param name="context">ClientContext to use to resolve '~sitecollection/' and '~site/' prefixes</param>
        /// <param name="prefixedPath">Path, optionally starting with '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>Server relative path, with the prefix replaced with the corresponding server relative path of the site or web; if there is no prefix the original path is returned</returns>
        public static string ResolveServerRelativeUrl(ClientContext context, string prefixedPath)
        {
            return ResolveServerRelativeUrl(context.Site, context.Web, prefixedPath);
        }

        /// <summary>
        /// Resolves site collection and site relative URLs.
        /// </summary>
        /// <param name="site">Site to use if '~sitecollection/' prefix needs to be resolved</param>
        /// <param name="web">Web to use if '~site/' prefix needs to be resolved</param>
        /// <param name="prefixedPath">Path, starting with '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>Server relative path, with the prefix replaced with the corresponding server relative path of the site or web; if there is no prefix the original path is returned</returns>
        public static string ResolveServerRelativeUrl(Site site, Web web, string prefixedPath)
        {
            const int LengthOfSitePrefix = 6;
            const int LengthOfSiteCollectionPrefix = 16;
            string path = null;
            if (prefixedPath.StartsWith("~site/", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!web.IsObjectPropertyInstantiated("ServerRelativeUrl"))
                {
                    web.Context.Load(web, w => w.ServerRelativeUrl);
                    web.Context.ExecuteQuery();
                }
                path = Combine(web.ServerRelativeUrl, prefixedPath.Substring(LengthOfSitePrefix));
            }
            else if (prefixedPath.StartsWith("~sitecollection/", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!site.IsObjectPropertyInstantiated("ServerRelativeUrl"))
                {
                    site.Context.Load(site, s => s.ServerRelativeUrl);
                    site.Context.ExecuteQuery();
                }
                path = Combine(site.ServerRelativeUrl, prefixedPath.Substring(LengthOfSiteCollectionPrefix));
            }
            else
            {
                path = prefixedPath;
            }
            return path;
        }

    }
}
