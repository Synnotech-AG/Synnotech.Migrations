using System;
using System.Data;
using System.Threading.Tasks;
using Light.GuardClauses;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.DatabaseAbstractions;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.Linq2Db
{
    /// <summary>
    /// Represents a generic base class for sessions that retrieves and stores migration info instances
    /// via a Linq2Db data connection. You can derive from this type and add all other dependencies
    /// that your migrations need for execution.
    /// </summary>
    /// <typeparam name="TDataConnection">Your custom database context type that derives from <see cref="DataConnection" />.</typeparam>
    /// <typeparam name="TMigrationInfo">The model type that represents migration infos in the target database.</typeparam>
    public abstract class MigrationSession<TDataConnection, TMigrationInfo> : IMigrationSession<TMigrationInfo>
        where TDataConnection : DataConnection
        where TMigrationInfo : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession{TDataConnection,TMigrationInfo}" />.
        /// </summary>
        /// <param name="dataConnection">The Linq2Db data connection used to access the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection"/> is null.</exception>
        protected MigrationSession(TDataConnection dataConnection) =>
            DataConnection = dataConnection.MustNotBeNull(nameof(dataConnection));

        /// <summary>
        /// Gets the Linq2Db data connection that allows access to the target database.
        /// </summary>
        public TDataConnection DataConnection { get; }

        /// <summary>
        /// Starts a new transaction with the specified transaction level.
        /// </summary>
        /// <param name="level">The locking behavior of the transaction. The default level is <see cref="IsolationLevel.Serializable"/>.</param>
        public Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.Serializable) => DataConnection.BeginTransactionAsync(level);

        /// <summary>
        /// Disposes the data connection which in turn will close a pending transaction if one was created.
        /// </summary>
        void IDisposable.Dispose() => DataConnection.Dispose();

        /// <summary>
        /// Executes the specified SQL statement against the target database and
        /// return the number of affected records.
        /// </summary>
        /// <param name="sql">The SQL script that should be executed.</param>
        public Task<int> ExecuteScriptAsync(string sql) => DataConnection.ExecuteAsync(sql);

        Task IAsyncSession.SaveChangesAsync() => DataConnection.CommitTransactionAsync();

        /// <summary>
        /// Gets the latest migration info from the target database.
        /// </summary>
        protected abstract Task<TMigrationInfo?> GetLatestMigrationInfoAsync();

        Task<TMigrationInfo?> IMigrationSession<TMigrationInfo>.GetLatestMigrationInfoAsync() =>
            GetLatestMigrationInfoAsync();

        Task IMigrationSession<TMigrationInfo>.StoreMigrationInfoAsync(TMigrationInfo migrationInfo) =>
            DataConnection.InsertAsync(migrationInfo);

        /// <summary>
        /// Disposes the data connection which in turn will close a pending transaction if one was created.
        /// </summary>
        ValueTask IAsyncDisposable.DisposeAsync() => DataConnection.DisposeAsync();
    }
}