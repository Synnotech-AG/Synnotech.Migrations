using System.Threading.Tasks;
using Raven.Client.Documents;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default factory for RavenDB migration sessions with text versions.
    /// </summary>
    public sealed class SessionFactory : ISessionFactory<MigrationSession, Migration>
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
        public ValueTask<MigrationSession> CreateSessionForMigrationAsync(Migration migration) => CreateSession();
        
        /// <summary>
        /// Creates a new <see cref="MigrationSession" /> instance using a new session from the underlying <see cref="IDocumentStore" />.
        /// </summary>
        public ValueTask<MigrationSession> CreateSessionForRetrievingLatestMigrationInfoAsync() => CreateSession();
        
        private ValueTask<MigrationSession> CreateSession() => new (new MigrationSession(_store.OpenAsyncSession()));
    }
}