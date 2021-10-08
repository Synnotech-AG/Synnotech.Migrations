using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.Linq2Db;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the session that is used to apply a single migration and store a corresponding migration info via LinqToDB.
    /// </summary>
    /// <typeparam name="TDataConnection">Your custom subtype that derives from <see cref="DataConnection" />.</typeparam>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
    public class LinqToDbMigrationSession<TDataConnection, TMigrationInfo> : AsyncSession<TDataConnection>, IMigrationSession<TDataConnection, TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
        where TDataConnection : DataConnection
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbMigrationSession{TDataConnection, TMigrationInfo}" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbMigrationSession(TDataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Returns the underlying LinqToDB data connection that is used as the context for migrations.
        /// </summary>
        public TDataConnection Context => DataConnection;

        /// <summary>
        /// Inserts the specified migration info into the database. If the database is configured to generate
        /// an ID for the migration info when this statement is executed, it will not be written back to the
        /// migration info entity.
        /// </summary>
        public virtual ValueTask StoreMigrationInfoAsync(TMigrationInfo migrationInfo, CancellationToken cancellationToken = default) =>
            new (DataConnection.InsertAsync(migrationInfo, token: cancellationToken));
    }

    /// <summary>
    /// Represents the session that is used to apply a single migration and store a corresponding migration info via LinqToDB.
    /// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
    /// </summary>
    /// <typeparam name="TDataConnection">Your custom subtype that derives from <see cref="DataConnection" />.</typeparam>
    public class LinqToDbMigrationSession<TDataConnection> : LinqToDbMigrationSession<TDataConnection, MigrationInfo>
        where TDataConnection : DataConnection
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbMigrationSession{TDataConnection}" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbMigrationSession(TDataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Stores the specified migration info in the database. The generated ID is written back to the Id property
        /// of the migration info object.
        /// </summary>
        public override async ValueTask StoreMigrationInfoAsync(MigrationInfo migrationInfo, CancellationToken cancellationToken = default) =>
            migrationInfo.Id = await DataConnection.InsertWithInt32IdentityAsync(migrationInfo, token: cancellationToken);
    }

    /// <summary>
    /// Represents the session that is used to apply a single migration and store a corresponding migration info via LinqToDB.
    /// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
    /// </summary>
    public sealed class LinqToDbMigrationSession : LinqToDbMigrationSession<DataConnection>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbMigrationSession" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbMigrationSession(DataConnection dataConnection) : base(dataConnection) { }
    }
}