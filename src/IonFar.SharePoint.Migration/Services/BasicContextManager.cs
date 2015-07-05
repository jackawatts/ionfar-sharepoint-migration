using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Net;
using System.Security;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// A simple context manager that creates a new ClientContext when the migration starts and disposes it afterwards.
    /// </summary>
    public class BasicContextManager : IContextManager
    {
        ClientContext _context;
        IUpgradeLog _log;
        SecureString _password;
        string _sharePointUrl;
        string _userName;

        /// <summary>
        /// Creates a basic context manager
        /// </summary>
        /// <param name="sharePointUrl">URL to use for the context</param>
        /// <param name="userName">User name to use</param>
        /// <param name="userName">Password to use</param>
        public BasicContextManager(string sharePointUrl, string userName, string password)
        {
            _sharePointUrl = sharePointUrl;
            _userName = userName;
            _password = GetSecureStringFromString(password);
        }

        /// <summary>
        /// Creates a basic context manager
        /// </summary>
        /// <param name="sharePointUrl">URL to use for the context</param>
        /// <param name="userName">User name to use</param>
        /// <param name="userName">Password to use</param>
        public BasicContextManager(string sharePointUrl, string userName, SecureString password)
        {
            _sharePointUrl = sharePointUrl;
            _userName = userName;
            _password = password;
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
        public SecureString SecurePassword
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets the username
        /// </summary>
        public string UserName
        {
            get
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
            _log.Verbose("Creating context {0}", _sharePointUrl);
            _context = new ClientContext(_sharePointUrl);
            var credentials = new SharePointOnlineCredentials(_userName, _password);
            _context.Credentials = credentials;
            return new BasicContextDisposer(this);
        }
        
        /// <summary>
        /// Converts plain text to a secure string
        /// </summary>
        public static SecureString GetSecureStringFromString(string nonsecureString)
        {
            var result = new SecureString();
            foreach (char c in nonsecureString)
            {
                result.AppendChar(c);
            }

            return result;
        }

        // Wrapper to dispose of the context
        class BasicContextDisposer : IDisposable
        {
            BasicContextManager _contextManager;

            public BasicContextDisposer(BasicContextManager contextManager)
            {
                _contextManager = contextManager;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        var log = _contextManager._log;
                        var clientContext = _contextManager._context;
                        _contextManager._log = null;
                        _contextManager._context = null;
                        log.Verbose("Disposing context {0}", clientContext.Url);
                        clientContext.Dispose();
                    }
                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~BasicContextDisposer() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion

        }
    }
}
