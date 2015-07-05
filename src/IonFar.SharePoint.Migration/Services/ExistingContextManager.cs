using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Security;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// A context manager that uses an existing ClientContext.
    /// </summary>
    public class ExistingContextManager : IContextManager
    {
        ClientContext _context;
        IUpgradeLog _log;
        SecureString _password;
        string _userName;

        public ExistingContextManager(ClientContext existingContext, string userName, SecureString password)
        {
            _context = existingContext;
            _password = password;
            _userName = userName;
        }

        /// <summary>
        /// Gets the current context from the context manager
        /// </summary>
        public ClientContext CurrentContext
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// Gets the secured password
        /// </summary>
        public SecureString SecurePassword { get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets the username
        /// </summary>
        public string UserName { get
            {
                return _userName;
            }
        }

        /// <summary>
        /// Tells the context manager when it is starting, and completing, a migration context
        /// </summary>
        /// <param name="log">Log to use</param>
        public IDisposable ContextScope(IUpgradeLog log)
        {
            _log = log;
            return new NullDisposable();
        }

        class NullDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
