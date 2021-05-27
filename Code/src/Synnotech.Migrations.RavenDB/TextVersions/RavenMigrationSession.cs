using System;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.RavenDB;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the session that is used to apply a migration and store the corresponding migration info.
    /// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
    /// </summary>
    public sealed class RavenMigrationSession : AsyncSession, IMigrationSession<IAsyncDocumentSession, MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenMigrationSession" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public RavenMigrationSession(IAsyncDocumentSession session) : base(session) { }

        /// <summary>
        /// Returns the underlying RavenDB <see cref="IAsyncDocumentSession" /> that is used as the context
        /// for migrations.
        /// </summary>
        public IAsyncDocumentSession Context => Session;

        /// <summary>
        /// Stores the specified migration info in the database.
        /// </summary>
        /// <param name="migrationInfo">The migration info object that will be stored in the database.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public ValueTask StoreMigrationInfoAsync(MigrationInfo migrationInfo, CancellationToken cancellationToken = default) =>
            new (Session.StoreAsync(migrationInfo,
                                    "migrationInfos" + Session.Advanced.DocumentStore.Conventions.IdentityPartsSeparator + migrationInfo.Version,
                                    cancellationToken));
    }
}