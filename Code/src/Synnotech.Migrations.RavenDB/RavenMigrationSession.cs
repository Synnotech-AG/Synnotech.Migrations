using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.RavenDB;

namespace Synnotech.Migrations.RavenDB
{
    /// <summary>
    /// Represents the session that is used apply a migration and store the corresponding migration info.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info.</typeparam>
    public class RavenMigrationSession<TMigrationInfo> : AsyncSession, IMigrationSession<IAsyncDocumentSession, TMigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenMigrationSession{TMigrationInfo}" />.
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
        public ValueTask StoreMigrationInfoAsync(TMigrationInfo migrationInfo) =>
            new (Session.StoreAsync(migrationInfo));
    }
}