using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Manages the connection to SharePoint use for migrations (and journaling)
    /// </summary>
    public interface IContextManager
    {
        /// <summary>
        /// Gets the current context from the context manager
        /// </summary>
        ClientContext CurrentContext { get; }

        /// <summary>
        /// Gets the secured password
        /// </summary>
        SecureString SecurePassword { get; }

        /// <summary>
        /// Gets the username
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Tells the context manager when it is starting, and completing, a migration context
        /// </summary>
        /// <param name="log">Log to use</param>
        IDisposable ContextScope(IUpgradeLog log);

    }
}
