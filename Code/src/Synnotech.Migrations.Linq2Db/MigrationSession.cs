using System;
using System.Data;
using System.Threading.Tasks;
using Light.GuardClauses;
using LinqToDB;
using LinqToDB.Data;
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
    public abstract class MigrationSession<TDataConnection, TMigrationInfo> : IAsyncMigrationSession<TMigrationInfo>
        where TDataConnection : DataConnection
        where TMigrationInfo : class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession{TDataConnection,TMigrationInfo}" />.
        /// </summary>
        /// <param name="dataConnection">The Linq2Db data connection used to access the target database.</param>
        /// <param name="transactionLevel">The transaction level. If null is specified, no transaction will be created.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection"/> is null.</exception>
        protected MigrationSession(TDataConnection dataConnection, IsolationLevel? transactionLevel = IsolationLevel.Serializable)
        {
            DataConnection = dataConnection.MustNotBeNull(nameof(dataConnection));
            if (transactionLevel != null)
                Transaction = DataConnection.BeginTransaction(transactionLevel.Value);
        }

        /// <summary>
        /// Gets the Linq2Db data connection that allows access to the target database.
        /// </summary>
        public TDataConnection DataConnection { get; }

        private DataConnectionTransaction? Transaction { get; }

        /// <summary>
        /// Disposes the data connection and the transaction (if one was created).
        /// </summary>
        public void Dispose()
        {
            Transaction?.Dispose();
            DataConnection.Dispose();
        }

        /// <summary>
        /// Executes the specified SQL statement against the target database and
        /// return the number of affected records.
        /// </summary>
        /// <param name="sql">The SQL script that should be executed.</param>
        public Task<int> ExecuteScriptAsync(string sql) => DataConnection.ExecuteAsync(sql);

        Task IAsyncSession.SaveChangesAsync() => Transaction?.CommitAsync() ?? Task.CompletedTask;

        /// <summary>
        /// Gets the latest migration info from the target database.
        /// </summary>
        protected abstract Task<TMigrationInfo?> GetLatestMigrationInfoAsync();

        Task<TMigrationInfo?> IAsyncMigrationSession<TMigrationInfo>.GetLatestMigrationInfoAsync() =>
            GetLatestMigrationInfoAsync();

        Task IAsyncMigrationSession<TMigrationInfo>.StoreMigrationInfoAsync(TMigrationInfo migrationInfo) =>
            DataConnection.InsertAsync(migrationInfo);
    }
}