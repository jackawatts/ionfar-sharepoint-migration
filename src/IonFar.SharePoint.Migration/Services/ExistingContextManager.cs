using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// A context manager that uses an existing ClientContext.
    /// </summary>
    public class ExistingContextManager : IContextManager
    {
        ClientContext _context;
        IUpgradeLog _log;

        public ExistingContextManager(ClientContext existingContext)
        {
            _context = existingContext;
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
