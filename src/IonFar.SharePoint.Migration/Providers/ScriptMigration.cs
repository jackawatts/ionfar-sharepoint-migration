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

        public ScriptMigration(string filePath)
        {
            _filePath = filePath;
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

                // TODO: Allow custom parameters to be passed through (from ScriptMigrationProvider)

                using (var shell = PowerShell.Create())
                {
                    shell.Runspace = runspace;
                    var pipeline = runspace.CreatePipeline();

                    shell.AddScript("Set-ExecutionPolicy Unrestricted -Scope CurrentUser;");
                    shell.AddScript("Import-Module OfficeDevPnP.PowerShell.Commands");

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
                    // TODO: Support custom parameters (from ScriptMigrationProvider)

                    // Exit codes not supported... need to check exit within script and convert to error
                    //shell.AddScript("Write-Host \"EXIT: $LastExitCode\";");

                    PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();
                    //outputCollection.DataAdded += OutputCollection_DataAdded;
                    outputCollection.DataAdded += outputLogger.OutputCollection_DataAdded;
                    shell.Streams.Debug.DataAdded += Debug_DataAdded;
                    shell.Streams.Error.DataAdded += Error_DataAdded;
                    //shell.Streams.Progress.DataAdded += Progress_DataAdded;
                    //shell.Streams.Verbose.DataAdded += Verbose_DataAdded;
                    //shell.Streams.Warning.DataAdded += Warning_DataAdded;

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
                        logger.Error("Script had errors. State: {0}", shell.InvocationStateInfo.State);
                        foreach (var error in shell.Streams.Error)
                        {
                            logger.Error("{0}", error);
                        }
                        throw new ScriptException(string.Format("{0}. {1}", shell.InvocationStateInfo.State, shell.InvocationStateInfo.Reason));
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


        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("@Debug: {0}", item);
            }
            else
            {
                Console.WriteLine("@Debug");
            }
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("@Error: {0}", item);
            }
            else
            {
                Console.WriteLine("@Error");
            }
        }

        //private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        //{
        //    var items = sender as IList;
        //    if (items != null)
        //    {
        //        var item = items[e.Index];
        //        Console.WriteLine("@Progress: {0}", item);
        //    }
        //    else
        //    {
        //        Console.WriteLine("@Progress");
        //    }
        //}

        //private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        //{
        //    var items = sender as IList;
        //    if (items != null)
        //    {
        //        var item = items[e.Index];
        //        Console.WriteLine("@Verbose: {0}", item);
        //    }
        //    else
        //    {
        //        Console.WriteLine("@Verbose");
        //    }
        //}

        //private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        //{
        //    var items = sender as IList;
        //    if (items != null)
        //    {
        //        var item = items[e.Index];
        //        Console.WriteLine("@Warning: {0}", item);
        //    }
        //    else
        //    {
        //        Console.WriteLine("@Warning");
        //    }
        //}

        //private void OutputCollection_DataAdded(object sender, DataAddedEventArgs e)
        //{
        //    var items = sender as IList;
        //    if (items != null)
        //    {
        //        var item = items[e.Index];
        //        Console.WriteLine("@Output: {0}", item);
        //    }
        //    else
        //    {
        //        Console.WriteLine("@Output");
        //    }
        //}

        class OutputLogger
        {
            IUpgradeLog _log;

            public OutputLogger(IUpgradeLog logger)
            {
                _log = logger;
            }

            public void OutputCollection_DataAdded(object sender, DataAddedEventArgs e)
            {
                try
                {
                    var items = (IList)sender;
                    var item = items[e.Index];
                    _log.Information("{0}", item);
                }
                catch(Exception ex)
                {
                    _log.Warning("Exception logging output: {0}", ex);
                }
            }
        }
    }
}
