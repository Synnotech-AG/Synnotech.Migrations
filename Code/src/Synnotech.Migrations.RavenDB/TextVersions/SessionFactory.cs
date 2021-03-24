using Raven.Client.Documents;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default factory for async migration sessions.
    /// </summary>
    public sealed class SessionFactory : IAsyncSessionFactory<MigrationSession, Migration>
    {
        private readonly IDocumentStore _store;

        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactory" />.
        /// </summary>
        /// <param name="store">The RavenDB document store that is used to create session instances.</param>
        public SessionFactory(IDocumentStore store) => _store = store;

        /// <summary>
        /// Creates a new <see cref="MigrationSession" /> instance using a new session from the underlying <see cref="IDocumentStore" />.
        /// </summary>
        public MigrationSession CreateSessionForMigration(Migration migration) => CreateSession();
        
        /// <summary>
        /// Creates a new <see cref="MigrationSession" /> instance using a new session from the underlying <see cref="IDocumentStore" />.
        /// </summary>
        public MigrationSession CreateSessionForRetrievingLatestMigrationInfo() => CreateSession();
        

        private MigrationSession CreateSession() => new(_store.OpenAsyncSession());
    }
}