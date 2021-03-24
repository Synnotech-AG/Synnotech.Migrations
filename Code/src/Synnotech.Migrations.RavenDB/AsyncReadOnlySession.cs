using System;
using Light.GuardClauses;
using Raven.Client.Documents.Session;

namespace Synnotech.Migrations.RavenDB
{
    /// <summary>
    /// Represents an async session with a RavenDB server that has read-only access
    /// (i.e. it cannot commit changes to the server).
    /// IMPORTANT: you cannot derive from this class and introduce
    /// new disposable references as <see cref="Dispose"/> is not virtual!
    /// </summary>
    public abstract class AsyncReadOnlySession : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncReadOnlySession"/>.
        /// </summary>
        /// <param name="session">The document session that prepares RQL statements for the server.</param>
        protected AsyncReadOnlySession(IAsyncDocumentSession session) =>
            Session = session.MustNotBeNull(nameof(session));

        /// <summary>
        /// Gets the session that is used to interact with the RavenDB server.
        /// </summary>
        protected IAsyncDocumentSession Session { get; }

        /// <summary>
        /// Disposes the underlying <see cref="IAsyncDocumentSession"/>.
        /// </summary>
        public void Dispose() => Session.Dispose();
    }
}