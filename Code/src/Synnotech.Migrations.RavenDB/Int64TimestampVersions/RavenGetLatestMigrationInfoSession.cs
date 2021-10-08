using System;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.RavenDB;

namespace Synnotech.Migrations.RavenDB.Int64TimestampVersions
{
    /// <summary>
    /// Represents the session that retrieves the latest migration info from RavenDB.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
    public class RavenGetLatestMigrationInfoSession<TMigrationInfo> : AsyncReadOnlySession, IGetLatestMigrationInfoSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenGetLatestMigrationInfoSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        public RavenGetLatestMigrationInfoSession(IAsyncDocumentSession session) : base(session) { }

        /// <summary>
        /// Gets the latest migration info stored in the target RavenDB, or null if no migrations have been
        /// applied. The latest migration info is determined by the highest long value.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="RavenException">Thrown when any communication error with the database occurs.</exception>
        public Task<TMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken = default) =>
#nullable disable
            Session.Query<TMigrationInfo>()
                   .OrderByDescending(migrationInfo => migrationInfo.Version)
                   .FirstOrDefaultAsync(cancellationToken);
#nullable restore
    }

    /// <summary>
    /// Represents the session that retrieves the latest migration info from RavenDB.
    /// <see cref="MigrationInfo" /> is used as the type that represents stored migration infos.
    /// </summary>
    public sealed class RavenGetLatestMigrationInfoSession : RavenGetLatestMigrationInfoSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenGetLatestMigrationInfoSession" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        public RavenGetLatestMigrationInfoSession(IAsyncDocumentSession session) : base(session) { }
    }
}