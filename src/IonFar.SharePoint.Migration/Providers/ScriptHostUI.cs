using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    class ScriptHostUI : PSHostUserInterface
    {
        PSHostRawUserInterface _hostRawUI;

        public ScriptHostUI() {
            _hostRawUI = new ScriptHostRawUI();
        }


        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return _hostRawUI;
            }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException("Prompt not implemented");
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException("PromptForChoice not implemented");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException("PromptForCredential not implemented");
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException("PromptForCredential (types) not implemented");
        }

        public override string ReadLine()
        {
            throw new NotImplementedException("ReadLine not implemented");
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException("ReadLineAsSecureString not implemented");
        }

        public override void Write(string value)
        {
            Console.Write(value);
            //Console.WriteLine("*Write* {0}", value);
            //throw new NotImplementedException("Write not implemented");
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Console.Write(value);
            //Console.WriteLine("*Write FG={0} BG={1}* {2}", foregroundColor, backgroundColor, value);
            //throw new NotImplementedException("Write (color) not implemented");
        }

        public override void WriteDebugLine(string message)
        {
            throw new NotImplementedException("WriteDebugLine not implemented");
        }

        public override void WriteErrorLine(string value)
        {
            throw new NotImplementedException("WriteErrorLine not implemented");
        }

        public override void WriteLine(string value)
        {
            Console.WriteLine("*WriteLine*" + value);
            //throw new NotImplementedException();
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            throw new NotImplementedException("WriteProgress not implemented");
        }

        public override void WriteVerboseLine(string message)
        {
            throw new NotImplementedException("WriteVerboseLine not implemented");
        }

        public override void WriteWarningLine(string message)
        {
            throw new NotImplementedException("WriteWarningLine not implemented");
        }
    }
}
