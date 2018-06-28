using IonFar.SharePoint.Migration;
using IonFar.SharePoint.Migration.Services;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IonFar.SharePoint.Migration.Services
{
    public class AppOnlyContextManager : IContextManager
    {
        private ClientContext _context;
        private IUpgradeLog _log;
        private string _clientId;
        private string _clientSecret;
        private string _siteUrl;

        public AppOnlyContextManager(string siteUrl, string clientId, string clientSecret)
        {
            _siteUrl = siteUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            var authMgr = new AuthenticationManager();
            _context = authMgr.GetAppOnlyAuthenticatedContext(siteUrl, clientId, clientSecret);
        }

        public ClientContext CurrentContext
        {
            get
            {
                return _context;
            }
        }

        public string Password
        {
            get
            {
                return _clientSecret;
            }
        }

        public SecureString SecurePassword
        {
            get
            {
                return GetSecureStringFromString(_clientSecret);
            }
        }

        public string UserName
        {
            get
            {
                return _clientId;
            }
        }

        /// <summary>
        /// Tells the context manager when it is starting, and completing, a migration context
        /// </summary>
        /// <param name="log">Log to use</param>
        public IDisposable ContextScope(IUpgradeLog log)
        {
            _log = log;
            _log.Verbose("Creating context {0}", _siteUrl);
            var authMgr = new AuthenticationManager();
            _context = authMgr.GetAppOnlyAuthenticatedContext(_siteUrl, _clientId, _clientSecret);

            return new AppOnlyContextDisposer(this);
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
        class AppOnlyContextDisposer : IDisposable
        {
            AppOnlyContextManager _contextManager;

            public AppOnlyContextDisposer(AppOnlyContextManager contextManager)
            {
                _contextManager = contextManager;
            }

            #region IDisposable Support
            private bool _disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
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
                    _disposedValue = true;
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
