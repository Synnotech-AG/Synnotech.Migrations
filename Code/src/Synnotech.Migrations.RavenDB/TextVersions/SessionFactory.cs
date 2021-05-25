using System;
using System.Threading.Tasks;
using Light.GuardClauses;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default factory for RavenDB migration sessions with text versions.
    /// </summary>
    public sealed class SessionFactory : ISessionFactory<MigrationInfo, Migration, IAsyncDocumentSession>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactory" />.
        /// </summary>
        /// <param name="store">The RavenDB document store that is used to create session instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is null.</exception>
        public SessionFactory(IDocumentStore store) => Store = store.MustNotBeNull(nameof(store));

        private IDocumentStore Store { get; }

        /// <summary>
        /// Creates the session that is used to retrieve the latest migration info from the target RavenDB database.
        /// </summary>
        public ValueTask<IGetLatestMigrationInfoSession<MigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync() =>
            new (new RavenGetLatestMigrationInfoSession(Store.OpenAsyncSession()));

        /// <summary>
        /// Creates the session that is used to apply a migration and store the corresponding migration info in the target database.
        /// </summary>
        /// <param name="migration">This value is ignored.</param>
        public ValueTask<IMigrationSession<IAsyncDocumentSession, MigrationInfo>> CreateSessionForMigrationAsync(Migration migration) =>
            new (new RavenMigrationSession(Store.OpenAsyncSession()));
    }
}