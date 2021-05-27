using System;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
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
        /// <param name="take">
        /// The amount of migration infos that are retrieved from the database (ordered by AppliedAt). The default value is 50.
        /// The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done as string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="take" /> is equal or less than 0.</exception>
        public RavenGetLatestMigrationInfoSession(IAsyncDocumentSession session, int take = 50) : base(session) =>
            Take = take.MustBeGreaterThan(0);

        private int Take { get; }

        /// <summary>
        /// Gets the latest migration info stored in the target RavenDB, or null if no migrations have been
        /// applied. The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done as string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="RavenException">Thrown when any communication error with the database occurs.</exception>
        public async Task<TMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken = default)
        {
            var migrationInfos = await Session.Query<TMigrationInfo>()
                                              .OrderByDescending(migrationInfo => migrationInfo.AppliedAt)
                                              .Take(Take)
                                              .ToListAsync(cancellationToken);
            return migrationInfos.GetLatestMigrationInfo();
        }
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
        /// <param name="take">
        /// The amount of migration infos that are retrieved from the database (ordered by AppliedAt). The default value is 50.
        /// The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done as string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="take" /> is equal or less than 0.</exception>
        public RavenGetLatestMigrationInfoSession(IAsyncDocumentSession session, int take = 100) : base(session, take) { }
    }
}