using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using LinqToDB.Data;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default factory for creating Linq2Db migration sessions that support text versions.
    /// </summary>
    public sealed class SessionFactory : ISessionFactory<MigrationInfo, Migration, DataConnection>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactory" />.
        /// </summary>
        /// <param name="createDataConnection">The factory that creates a new Linq2Db data connection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createDataConnection" /> is null.</exception>
        public SessionFactory(Func<DataConnection> createDataConnection) =>
            CreateDataConnection = createDataConnection.MustNotBeNull(nameof(createDataConnection));

        private Func<DataConnection> CreateDataConnection { get; }

        /// <summary>
        /// Gets or sets the number of entries that the <see cref="LinqToDbGetLatestMigrationInfoSession" /> will
        /// load from the database to determine the newest migrations. The default value is 100.
        /// This is done as string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit. The actual newest version is determined on the client-side.
        /// </summary>
        public int Take { get; set; } = 100;

        /// <summary>
        /// Creates the session that is used to retrieve the latest migration info from the target database.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public ValueTask<IGetLatestMigrationInfoSession<MigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken = default) =>
            new (new LinqToDbGetLatestMigrationInfoSession(CreateDataConnection(), Take));

        /// <summary>
        /// Creates the session that is used to apply a migration and store the corresponding migration info in the target database.
        /// </summary>
        /// <param name="migration">
        /// The migration to be executed. <see cref="Migration.IsRequiringTransaction" /> is used to determine if a transaction
        /// is started on the data connection.
        /// </param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="migration" /> is null.</exception>
        public async ValueTask<IMigrationSession<DataConnection, MigrationInfo>> CreateSessionForMigrationAsync(Migration migration,
                                                                                                                CancellationToken cancellationToken = default)
        {
            migration.MustNotBeNull(nameof(migration));

            var dataConnection = CreateDataConnection();
            if (migration.IsRequiringTransaction)
                await dataConnection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            return new LinqToDbMigrationSession(dataConnection);
        }

        /// <summary>
        /// Creates the session that is used to retrieve all migration infos from the target database.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public ValueTask<IGetAllMigrationInfosSession<MigrationInfo>> CreateSessionForRetrievingAllMigrationInfosAsync(CancellationToken cancellationToken = default) =>
            new (new LinqToDbGetAllMigrationInfosSession(CreateDataConnection()));
    }
}