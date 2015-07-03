using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    public class UrlTokenPreprocessor : ITextFilePreprocessor
    {
        private static readonly Regex tokenRegex = new Regex(@"~site/|~sitecollection/");
        private Site _site;
        private Web _web;

        /// <summary>
        /// Creates a UrlTokenPreprocessor for the site and web specified by the context
        /// </summary>
        public UrlTokenPreprocessor(ClientContext clientContext)
            : this(clientContext.Site, clientContext.Web)
        {
        }

        /// <summary>
        /// Creates a UrlTokenPreprocessor for the specified site and web
        /// </summary>
        public UrlTokenPreprocessor(Site site, Web web)
        {
            _site = site;
            _web = web;
        }

        /// <summary>
        /// Replaces ~sitecollection/ and ~site/ references
        /// </summary>
        public string Process(string contents)
        {
            var webUrl = SPUrlUtility.ResolveServerRelativeUrl(_site, _web, "~site/");
            if (!webUrl.EndsWith("/")) { webUrl += "/"; }
            var siteUrl = SPUrlUtility.ResolveServerRelativeUrl(_site, _web, "~sitecollection/");
            if (!siteUrl.EndsWith("/")) { siteUrl += "/"; }
            var result = tokenRegex.Replace(contents,
                match => (match.Value == "~site/") ? webUrl : siteUrl);
            return result;
        }
    }

}
