using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace IonFar.SharePoint.Migration.Providers.Script
{
    public class ScriptHostUI : PSHostUserInterface
    {
        PSHostRawUserInterface _hostRawUI;
        IUpgradeLog _log;

        public ScriptHostUI(IUpgradeLog logger) {
            _log = logger;
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
            //_log.Write(value);
            //Console.Write("##" + value);
            throw new NotImplementedException("Write not implemented");
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            // NOTE: Write-Host is output via a call to Write, then WriteLine (reset color)
            _log.Verbose("HOST: {0}", value);

            //_log.Write(value);
            //Console.Write("$$" + value);
            //Console.WriteLine("*Write FG={0} BG={1}* {2}", foregroundColor, backgroundColor, value);
            //throw new NotImplementedException("Write (color) not implemented");
        }

        public override void WriteDebugLine(string message)
        {
            // DO NOTHING - Logged via DataAdded handler

            //throw new NotImplementedException("WriteDebugLine not implemented");
        }

        public override void WriteErrorLine(string value)
        {
            // DO NOTHING - Logged via DataAdded handler

            //throw new NotImplementedException("WriteErrorLine not implemented");
        }

        public override void WriteLine(string value)
        {
            //Console.WriteLine("*WriteLine* {0}", value);
            throw new NotImplementedException("WriteLine not implemented");
        }

        public override void WriteLine()
        {
            throw new NotImplementedException("WriteLine (empty) not implemented");
            //base.WriteLine();
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            // NOTE: Write-Host is output via a call to Write, then WriteLine (reset color)
            if (!string.IsNullOrWhiteSpace(value))
            {
                _log.Verbose("HOST: {0}", value);
            }

            //throw new NotImplementedException("WriteLine (color) not implemented");
            //base.WriteLine(foregroundColor, backgroundColor, value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            // DO NOTHING - Logged via DataAdded handler

            //_log.Information("Progress: {0}", record);
            //Console.WriteLine("*Progress* {0} {1} {2} {3}", record.ActivityId, record.Activity, record.StatusDescription, record.PercentComplete);
            //throw new NotImplementedException("WriteProgress not implemented");
        }

        public override void WriteVerboseLine(string message)
        {
            // DO NOTHING - Logged via DataAdded handler

            //_log.Verbose(message);
            //Console.WriteLine("*Verbose* {0}", message);
            //throw new NotImplementedException("WriteVerboseLine not implemented");
        }

        public override void WriteWarningLine(string message)
        {
            // DO NOTHING - Logged via DataAdded handler

            //_log.Warning(message);
            //Console.WriteLine("*Warning* {0}", message);
            //throw new NotImplementedException("WriteWarningLine not implemented");
        }
    }
}
