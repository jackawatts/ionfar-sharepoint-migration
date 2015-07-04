using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Sync
{
    public class UrlTokenPreprocessor : ITextFilePreprocessor
    {
        private static readonly Regex tokenRegex = new Regex(@"~site/|~sitecollection/");

        /// <summary>
        /// Replaces ~sitecollection/ and ~site/ references
        /// </summary>
        public string Process(IContextManager contextManager, IUpgradeLog logger, string contents)
        {
            var context = contextManager.CurrentContext;

            var webUrl = SPUrlUtility.ResolveServerRelativeUrl(context, "~site/");
            if (!webUrl.EndsWith("/")) { webUrl += "/"; }
            var siteUrl = SPUrlUtility.ResolveServerRelativeUrl(context, "~sitecollection/");
            if (!siteUrl.EndsWith("/")) { siteUrl += "/"; }
            var result = tokenRegex.Replace(contents,
                match => (match.Value == "~site/") ? webUrl : siteUrl);
            return result;
        }
    }

}
