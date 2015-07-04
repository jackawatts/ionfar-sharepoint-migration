using System;
using System.Collections;
using System.Collections.Generic;
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
            var script = System.IO.File.ReadAllText(_filePath);

            var host = new ScriptHost();

            using (var runspace = RunspaceFactory.CreateRunspace(host))
            {
                runspace.Open();
                using (var shell = PowerShell.Create())
                {
                    shell.Runspace = runspace;

                    shell.AddScript("$ErrorActionPreference = 'Stop';");
                    shell.AddScript(script);

                    // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                    //shell.AddParameter("Url", contextManager.CurrentContext.Url);
                    //shell.AddParameter("Credentials", contextManager.CurrentContext.Credentials);

                    PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();
                    outputCollection.DataAdded += OutputCollection_DataAdded; ;
                    shell.Streams.Debug.DataAdded += Debug_DataAdded;
                    shell.Streams.Error.DataAdded += Error_DataAdded;
                    shell.Streams.Progress.DataAdded += Progress_DataAdded;
                    shell.Streams.Verbose.DataAdded += Verbose_DataAdded;
                    shell.Streams.Warning.DataAdded += Warning_DataAdded;

                    IAsyncResult result = shell.BeginInvoke<PSObject, PSObject>(null, outputCollection);

                    while (result.IsCompleted == false)
                    {
                        Console.WriteLine("Waiting for pipeline to finish...");
                        Thread.Sleep(1000);

                        // might want to place a timeout here...
                    }

                    Console.WriteLine("Execution has stopped. Errors: {0}. Pipeline state: {1}, Reason: {2}", shell.HadErrors, shell.InvocationStateInfo.State, shell.InvocationStateInfo.Reason);
                    Console.WriteLine("Host. ShouldExit: {0}. ExitCode: {1}", host.ShouldExit, host.ExitCode);

                    foreach (PSObject outputItem in outputCollection)
                    {
                        //TODO: handle/process the output items if required
                        Console.WriteLine(outputItem.BaseObject.ToString());
                    }

                    if (shell.HadErrors)
                    {
                        throw new ScriptException(string.Format("{0}. {1}", shell.InvocationStateInfo.State, shell.InvocationStateInfo.Reason));
                    }
                    if (host.ExitCode != 0)
                    {
                        throw new ScriptException(string.Format("Script exited with code: {0}", host.ExitCode));
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
                Console.WriteLine("Debug: {0}", item);
            }
            else
            {
                Console.WriteLine("Debug");
            }
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("Error: {0}", item);
            }
            else
            {
                Console.WriteLine("Error");
            }
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("Progress: {0}", item);
            }
            else
            {
                Console.WriteLine("Progress");
            }
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("Verbose: {0}", item);
            }
            else
            {
                Console.WriteLine("Verbose");
            }
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("Warning: {0}", item);
            }
            else
            {
                Console.WriteLine("Warning");
            }
        }

        private void OutputCollection_DataAdded(object sender, DataAddedEventArgs e)
        {
            var items = sender as IList;
            if (items != null)
            {
                var item = items[e.Index];
                Console.WriteLine("Output: {0}", item);
            }
            else
            {
                Console.WriteLine("Output");
            }
        }
    }
}
