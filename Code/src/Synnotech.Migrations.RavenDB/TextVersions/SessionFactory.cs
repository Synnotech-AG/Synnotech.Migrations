using System;
using System.Threading;
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store" /> is null.</exception>
        public SessionFactory(IDocumentStore store) => Store = store.MustNotBeNull(nameof(store));

        private IDocumentStore Store { get; }

        /// <summary>
        /// Gets or sets the number of entries that the <see cref="RavenGetLatestMigrationInfoSession" /> will
        /// load from the database to determine the newest migrations. The default value is 100.
        /// This is done as string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit. The actual newest version is determined on the client-side.
        /// </summary>
        public int Take { get; set; } = 100;

        /// <summary>
        /// Creates the session that is used to retrieve the latest migration info from the target RavenDB database.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public ValueTask<IGetLatestMigrationInfoSession<MigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken = default) =>
            new (new RavenGetLatestMigrationInfoSession(Store.OpenAsyncSession(), Take));

        /// <summary>
        /// Creates the session that is used to apply a migration and store the corresponding migration info in the target database.
        /// </summary>
        /// <param name="migration">This value is ignored.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public ValueTask<IMigrationSession<IAsyncDocumentSession, MigrationInfo>> CreateSessionForMigrationAsync(Migration migration,
                                                                                                                 CancellationToken cancellationToken = default) =>
            new (new RavenMigrationSession(Store.OpenAsyncSession()));
    }
}