using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;
using Synnotech.RavenDB;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the session that retrieves all migration infos from RavenDB.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
    public class RavenGetAllMigrationInfosSession<TMigrationInfo> : AsyncReadOnlySession, IGetAllMigrationInfosSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenGetAllMigrationInfosSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public RavenGetAllMigrationInfosSession(IAsyncDocumentSession session) : base(session) { }

        /// <summary>
        /// Gets a list of all migration infos stored in the target RavenDB. The returned migrations are ordered by the
        /// date they were applied at.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="RavenException">Thrown when any communication error with the database occurs.</exception>
        public Task<List<TMigrationInfo>> GetAllMigrationInfosAsync(CancellationToken cancellationToken = default) =>
            Session.Query<TMigrationInfo>()
                   .OrderBy(migrationInfo => migrationInfo.AppliedAt)
                   .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Represents the session that retrieves all migration infos from RavenDB.
    /// <see cref="MigrationInfo" /> is used as the type that represents stored migration infos.
    /// </summary>
    public sealed class RavenGetAllMigrationInfosSession : RavenGetAllMigrationInfosSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenGetAllMigrationInfosSession" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public RavenGetAllMigrationInfosSession(IAsyncDocumentSession session) : base(session) { }
    }
}