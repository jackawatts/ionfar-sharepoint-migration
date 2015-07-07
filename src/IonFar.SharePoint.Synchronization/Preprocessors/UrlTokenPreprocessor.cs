using System.Text.RegularExpressions;
using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization.Preprocessors
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
