using System;
using System.Data;
using Light.GuardClauses;
using LinqToDB.Data;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default factory for creating migration sessions.
    /// </summary>
    public sealed class SessionFactory : IAsyncSessionFactory<MigrationSession, Migration>
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
        public MigrationSession CreateSessionForMigration(Migration migration)
        {
            migration.MustNotBeNull(nameof(migration));

            IsolationLevel? transactionLevel = null;
            if (migration.IsRequiringTransaction)
                transactionLevel = IsolationLevel.Serializable;
            return new(CreateDataConnection(), transactionLevel);
        }

        /// <summary>
        /// Creates a new session without a transaction.
        /// </summary>
        public MigrationSession CreateSessionForRetrievingLatestMigrationInfo() => new(CreateDataConnection(), null);
    }
}