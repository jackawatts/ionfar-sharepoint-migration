using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// Substitutes variables for values in text files
    /// </summary>
    public class VariableSubstitutionPreprocessor : ITextFilePreprocessor
    {
        private readonly IDictionary<string, string> variables;
        private static readonly Regex tokenRegex = new Regex(@"\$(?<variableName>\w+)\$");

        /// <summary>
        /// Initializes a new instance of the VariableSubstitutionPreprocessor class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableSubstitutionPreprocessor(IDictionary<string, string> variables)
        {
            this.variables = variables;
        }

        /// <summary>
        /// Substitutes variables 
        /// </summary>
        public string Process(string contents)
        {
            return tokenRegex.Replace(contents, match => ReplaceToken(match, variables));
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
