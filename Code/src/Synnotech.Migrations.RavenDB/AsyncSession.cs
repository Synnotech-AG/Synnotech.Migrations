using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.RavenDB
{
    /// <summary>
    /// Represents an async session with a RavenDB server that can commit changes to the server.
    /// IMPORTANT: you cannot derive from this class and introduce
    /// new disposable references as <see cref="AsyncReadOnlySession.Dispose"/> is not virtual!
    /// </summary>
    public abstract class AsyncSession : AsyncReadOnlySession, IAsyncSession
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncSession" />.
        /// </summary>
        /// <param name="session">The document session that prepares RQL statements for the server.</param>
        /// <param name="waitForIndexesAfterSaveChanges">The value indicating whether the RavenDB client waits for all indexes to be updated during a <see cref="SaveChangesAsync" /> call.</param>
        /// <exception cref="ArgumentNullException">Thrown when y<paramref name="session" /> is null.</exception>
        protected AsyncSession(IAsyncDocumentSession session, bool waitForIndexesAfterSaveChanges = true) : base(session)
        {
            if (waitForIndexesAfterSaveChanges)
                session.Advanced.WaitForIndexesAfterSaveChanges();
        }

        /// <summary>
        /// Commits all current changes to the Raven DB server.
        /// </summary>
        public Task SaveChangesAsync() => Session.SaveChangesAsync();
    }
}