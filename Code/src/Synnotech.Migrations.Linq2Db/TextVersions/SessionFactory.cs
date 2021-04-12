using System;
using System.Threading.Tasks;
using Light.GuardClauses;
using LinqToDB.Data;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default factory for creating Linq2Db migration sessions that support text versions.
    /// </summary>
    public sealed class SessionFactory : ISessionFactory<MigrationSession, Migration>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SessionFactory"/>.
        /// </summary>
        /// <param name="createDataConnection">The factory that creates a new Linq2Db data connection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createDataConnection"/> is null.</exception>
        public SessionFactory(Func<DataConnection> createDataConnection) =>
            CreateDataConnection = createDataConnection.MustNotBeNull(nameof(createDataConnection));

        private Func<DataConnection> CreateDataConnection { get; }

        /// <summary>
        /// Creates a new session for the specified migration. If <see cref="Migration.IsRequiringTransaction"/>
        /// is true, then a serializable transaction will be created.
        /// </summary>
        /// <param name="migration">The migration the session is created for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="migration"/> is null.</exception>
        public async ValueTask<MigrationSession> CreateSessionForMigrationAsync(Migration migration)
        {
            migration.MustNotBeNull(nameof(migration));
            var session = new MigrationSession(CreateDataConnection());
            if (migration.IsRequiringTransaction)
                await session.BeginTransactionAsync();
            return session;
        }

        /// <summary>
        /// Creates a new session without a transaction.
        /// </summary>
        public ValueTask<MigrationSession> CreateSessionForRetrievingLatestMigrationInfoAsync()
        {
            var session = new MigrationSession(CreateDataConnection());
            return new (session);
        }
    }
}