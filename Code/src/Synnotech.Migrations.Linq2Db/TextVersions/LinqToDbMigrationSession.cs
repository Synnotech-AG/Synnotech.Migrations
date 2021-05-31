using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.Linq2Db;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the session that is used to apply a migration and store the corresponding migration info.
    /// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
    /// </summary>
    public sealed class LinqToDbMigrationSession : AsyncReadOnlySession, IMigrationSession<DataConnection, MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbMigrationSession" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbMigrationSession(DataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Returns the underlying LinqToDB <see cref="DataConnection" /> that is used as the context
        /// for migrations.
        /// </summary>
        public DataConnection Context => DataConnection;

        /// <summary>
        /// Stores the specified migration info in the database.
        /// </summary>
        public async ValueTask StoreMigrationInfoAsync(MigrationInfo migrationInfo, CancellationToken cancellationToken = default) =>
            migrationInfo.Id = await DataConnection.InsertWithInt32IdentityAsync(migrationInfo, token: cancellationToken);

        /// <summary>
        /// Commits the underlying transaction to the database (if there is any).
        /// </summary>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            DataConnection.CommitTransactionAsync(cancellationToken);
    }
}