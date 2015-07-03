using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// Preprocessor that modifies text files before they are uploaded.
    /// </summary>
    public interface ITextFilePreprocessor
    {

        /// <summary>
        /// Performs some preprocessing step on a script
        /// </summary>
        string Process(string contents);
    }
}
