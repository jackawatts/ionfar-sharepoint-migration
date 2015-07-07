using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization.Preprocessors
{
    /// <summary>
    /// Substitutes variables for values in text files
    /// </summary>
    public class VariableSubstitutionPreprocessor : ITextFilePreprocessor
    {
        private readonly IDictionary<string, string> _variables;
        private static readonly Regex TokenRegex = new Regex(@"\$(?<variableName>\w+)\$");

        /// <summary>
        /// Initializes a new instance of the VariableSubstitutionPreprocessor class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableSubstitutionPreprocessor(IDictionary<string, string> variables)
        {
            this._variables = variables;
        }

        /// <summary>
        /// Substitutes variables 
        /// </summary>
        public string Process(IContextManager contextManager, IUpgradeLog logger, string contents)
        {
            return TokenRegex.Replace(contents, match => ReplaceToken(match, _variables));
        }

        private static string ReplaceToken(Match match, IDictionary<string, string> variables)
        {
            var variableName = match.Groups["variableName"].Value;
            if (!variables.ContainsKey(variableName))
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Variable {0} has no value defined", variableName));
            return variables[variableName];
        }
    }

}
