using System;
using Microsoft.SharePoint.Client;
using System.Security;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// A context manager that uses an existing ClientContext.
    /// </summary>
    public class ExistingContextManager : IContextManager
    {
        readonly ClientContext _context;
        readonly string _password;
        readonly SecureString _securePassword;
        readonly string _userName;

        /// <summary>
        /// Creates an instance
        /// </summary>
        public ExistingContextManager(ClientContext existingContext)
        {
            _context = existingContext;
        }

        /// <summary>
        /// Creates an instance; userName and password is needed for ScriptMigrationProvider
        /// </summary>
        public ExistingContextManager(ClientContext existingContext, string userName, string password)
        {
            _context = existingContext;
            _password = password;
            _securePassword = BasicContextManager.GetSecureStringFromString(password);
            _userName = userName;
        }

        /// <summary>
        /// Creates an instance; userName and password is needed for ScriptMigrationProvider
        /// </summary>
        public ExistingContextManager(ClientContext existingContext, string userName, SecureString securePassword)
        {
            _context = existingContext;
            _securePassword = securePassword;
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
        /// Gets the password
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets the secured password
        /// </summary>
        public SecureString SecurePassword { get
            {
                return _securePassword;
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
