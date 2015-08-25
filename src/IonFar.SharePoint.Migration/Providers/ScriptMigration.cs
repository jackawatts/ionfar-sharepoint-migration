using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Providers
{
    class ScriptMigration : IMigration
    {
        // See: http://blogs.msdn.com/b/kebab/archive/2014/04/28/executing-powershell-scripts-from-c.aspx
        //      https://msdn.microsoft.com/en-us/library/ee706551(v=vs.85).aspx

        string _filePath;
        Dictionary<string, object> _variables;

        public ScriptMigration(string filePath, IDictionary<string,object> variables)
        {
            _filePath = filePath;
            _variables = new Dictionary<string, object>(variables);
        }

        public string Name
        {
            get
            {
                var fileName = System.IO.Path.GetFileName(_filePath);
                return fileName;
            }
        }

        public string Note
        {
            get
            {
                return _filePath;
            }
        }

        public void Apply(IContextManager contextManager, IUpgradeLog logger)
        {
            // IDEA: The other alternative to a custom host is simply define function Write-Host
            var host = new ScriptHost(logger);
            var outputLogger = new OutputLogger(logger);
            var parameters = GetScriptParameterNames().ToList();

            using (var runspace = RunspaceFactory.CreateRunspace(host))
            {
                runspace.Open();
                //runspace.SessionStateProxy.SetVariable("ErrorActionPreference", "Stop");
                runspace.SessionStateProxy.SetVariable("SPContext", contextManager.CurrentContext);
                runspace.SessionStateProxy.SetVariable("SPUrl", contextManager.CurrentContext.Url);
                runspace.SessionStateProxy.SetVariable("SPUserName", contextManager.UserName);
                runspace.SessionStateProxy.SetVariable("SPPassword", contextManager.Password);
                runspace.SessionStateProxy.SetVariable("SPSecurePassword", contextManager.SecurePassword);
                PSCredential psCredential = null;
                if (!string.IsNullOrWhiteSpace(contextManager.UserName) && contextManager.SecurePassword != null)
                {
                    psCredential = new PSCredential(contextManager.UserName, contextManager.SecurePassword);
                }
                runspace.SessionStateProxy.SetVariable("SPCredentials", psCredential);
                runspace.SessionStateProxy.SetVariable("SPVariables", _variables);

                // TODO: Allow custom parameters to be passed through (from ScriptMigrationProvider)

                // TODO: Store calculated PSCredential with ScriptHost / ScriptHostUI, so it returns from PromptForCredential

                //var initial = InitialSessionState.Create();
                //initial.ImportPSModule("OfficeDevPnP.PowerShell.Commands");

                using (var pipeline = runspace.CreatePipeline("Set-ExecutionPolicy Unrestricted -Scope Process -Confirm:$false -Force"))
                {
                    var output = pipeline.Invoke();
                }
                using (var pipeline = runspace.CreatePipeline("Import-Module OfficeDevPnP.PowerShell.Commands"))
                {
                    var output = pipeline.Invoke();
                }

                using (var shell = PowerShell.Create())
                {
                    shell.Runspace = runspace;

                    //shell.AddScript("Set-ExecutionPolicy Unrestricted -Scope CurrentUser;");
                    //shell.AddScript("Import-Module OfficeDevPnP.PowerShell.Commands");

                    shell.AddCommand(_filePath);
                    if (parameters.Any(p => string.Equals(p, "Context", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        shell.AddParameter("Context", contextManager.CurrentContext);
                    }
                    if (parameters.Any(p => string.Equals(p, "Url", StringComparison.InvariantCultureIgnoreCase))) {
                        shell.AddParameter("Url", contextManager.CurrentContext.Url);
                    }
                    if (parameters.Any(p => string.Equals(p, "UserName", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        shell.AddParameter("UserName", contextManager.UserName);
                    }
                    if (parameters.Any(p => string.Equals(p, "Password", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        shell.AddParameter("Password", contextManager.Password);
                    }
                    if (parameters.Any(p => string.Equals(p, "SecurePassword", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        shell.AddParameter("SecurePassword", contextManager.SecurePassword);
                    }
                    if (parameters.Any(p => string.Equals(p, "Credentials", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        shell.AddParameter("Credentials", psCredential);
                    }
                    // Custom parameters
                    foreach (var kvp in _variables)
                    {
                        if (parameters.Any(p => string.Equals(p, kvp.Key, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            shell.AddParameter(kvp.Key, kvp.Value);
                        }
                    }

                    // Exit codes not supported... need to check exit within script and convert to error
                    //shell.AddScript("Write-Host \"EXIT: $LastExitCode\";");

                    PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();
                    //outputCollection.DataAdded += OutputCollection_DataAdded;
                    outputCollection.DataAdded += outputLogger.OutputCollection_DataAdded;
                    shell.Streams.Debug.DataAdded += outputLogger.Debug_DataAdded;
                    shell.Streams.Error.DataAdded += outputLogger.Error_DataAdded;
                    shell.Streams.Progress.DataAdded += outputLogger.Progress_DataAdded;
                    shell.Streams.Verbose.DataAdded += outputLogger.Verbose_DataAdded;
                    shell.Streams.Warning.DataAdded += outputLogger.Warning_DataAdded;

                    var settings = new PSInvocationSettings()
                    {
                        ErrorActionPreference = ActionPreference.Stop
                    };
                    IAsyncResult result = shell.BeginInvoke<PSObject, PSObject>(null, outputCollection, settings, null, null);

                    while (result.IsCompleted == false)
                    {
                        //Console.WriteLine("Waiting for pipeline to finish...");
                        Thread.Sleep(1000);

                        // TODO: Add timeout (configured from ScriptMigrationProvider)
                    }

                    //Console.WriteLine("** Host. ShouldExit: {0}, ExitCode: {1}, HadErrors: {0}. State: {1}, Reason: {2}", 
                    //    host.ShouldExit, host.ExitCode, shell.HadErrors, shell.InvocationStateInfo.State, shell.InvocationStateInfo.Reason);

                    //Console.WriteLine("Output (after script run):");
                    //foreach (PSObject outputItem in outputCollection)
                    //{
                    //    //TODO: handle/process the output items if required
                    //    Console.WriteLine(outputItem.BaseObject.ToString());
                    //}

                    if (host.ExitCode != 0)
                    {
                        logger.Error("Script exited with code: {0}", host.ExitCode);
                        if (shell.HadErrors)
                        {
                            foreach (var error in shell.Streams.Error)
                            {
                                logger.Error("{0}", error);
                            }
                        }
                        throw new ScriptException(string.Format("Script exited with code: {0}", host.ExitCode));
                    }
                    if (shell.HadErrors)
                    {
                        if (shell.Streams.Error.Count > 0)
                        {
                            logger.Error("Script had non-terminating errors. Suppress errors to allow script to run. State: {0}", shell.InvocationStateInfo.State);
                            foreach (var error in shell.Streams.Error)
                            {
                                logger.Error("{0}", error);
                            }
                            throw new ScriptException(string.Format("{0}. {1}", shell.InvocationStateInfo.State, shell.InvocationStateInfo.Reason));
                        }
                        else
                        {
                            logger.Warning("Script had non-terminating errors that were suppressed (error stream was empty). State: {0}", shell.InvocationStateInfo.State);
                        }
                    }
                    logger.Verbose("Script '{0}' complete", _filePath);
                }
            }
        }

        private IEnumerable<string> GetScriptParameterNames()
        {
            // https://nightroman.wordpress.com/2008/10/16/get-names-of-script-parameters/
            var script = System.IO.File.ReadAllText(_filePath);

            var mode = 0;
            var param = true;
            Collection<PSParseError> errors;
            var tokens = PSParser.Tokenize(script, out errors);

            for (var i = 0; i < tokens.Count; ++i)
            {
                var t = tokens[i];

                // Skip "[]" values
                if ((t.Type == PSTokenType.Operator) && (t.Content == "["))
                {
                    var level = 1;
                    for (++i; i < tokens.Count; ++i)
                    {
                        t = tokens[i];
                        if (t.Type == PSTokenType.Operator)
                        {
                            if (t.Content == "[")
                            {
                                ++level;
                            }
                            else if (t.Content == "]")
                            {
                                --level;
                                if (level <= 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    continue;
                }

                // Find and return parameter names
                switch (t.Type)
                {
                    case PSTokenType.NewLine: { break; }
                    case PSTokenType.Comment: { break; }
                    case PSTokenType.Command:
                        {
                            if (mode <= 1)
                            {
                                //yield return;
                            }
                            break;
                        }
                    case PSTokenType.Keyword:
                        {
                            if (mode == 0)
                            {
                                if (string.Equals(t.Content, "param", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    mode = 1;
                                }
                            }
                            break;
                        }
                    case PSTokenType.GroupStart:
                        {
                            if (mode > 0)
                            {
                                ++mode;
                            }
                            else
                            {
                                yield break;
                            }
                            break;
                        }
                    case PSTokenType.GroupEnd:
                        {
                            --mode;
                            if (mode < 2)
                            {
                                yield break;
                            }
                            break;
                        }
                    case PSTokenType.Variable:
                        {
                            if (mode == 2 && param)
                            {
                                param = false;
                                //  if ((!$Pattern) -or($t.Content - like $Pattern)) {
                                //$t.Content
                                //  }
                                yield return t.Content;
                            }
                            break;
                        }
                    case PSTokenType.Operator:
                        {
                            if ((mode == 2) && (t.Content == ","))
                            {
                                param = true;
                            }
                            break;
                        }
                }
            }
        }



        class OutputLogger
        {
            IUpgradeLog _log;

            public OutputLogger(IUpgradeLog logger)
            {
                _log = logger;
            }

            public void Debug_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Verbose("DEBUG: {0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging debug: {0}", ex);
                }
            }

            public void Error_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Error("{0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging error: {0}", ex);
                }
            }

            public void Progress_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Verbose("PROGRESS: {0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging progress: {0}", ex);
                }
            }

            public void OutputCollection_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Information("{0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging output: {0}", ex);
                }
            }

            public void Verbose_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Verbose("{0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging verbose: {0}", ex);
                }
            }

            public void Warning_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Warning("{0}", item);
                }
                catch (Exception ex)
                {
                    _log.Warning("Exception logging warning: {0}", ex);
                }
            }
        }
    }
}
