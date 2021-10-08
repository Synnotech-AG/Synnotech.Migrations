using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.Linq2Db;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.Linq2Db.Int64TimestampVersions
{
    /// <summary>
    /// Represents the session that retrieves the latest migration info for LinqToDB.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
    public class LinqToDbGetLatestMigrationInfoSession<TMigrationInfo> : AsyncReadOnlySession, IGetLatestMigrationInfoSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbGetLatestMigrationInfoSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbGetLatestMigrationInfoSession(DataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Gets the latest migration info stored in the target database, or null if no migrations have been
        /// applied. The latest migration is determined by its version.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public async Task<TMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await DataConnection.GetTable<TMigrationInfo>()
                                           .OrderByDescending(migrationInfo => migrationInfo.Version)
                                           .FirstOrDefaultAsync(cancellationToken);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Represents the session that retrieves the latest migration info for LinqToDB.
    /// <see cref="MigrationInfo" /> is used as the the type that represents stored migration infos.
    /// </summary>
    public sealed class LinqToDbGetLatestMigrationInfoSession : LinqToDbGetLatestMigrationInfoSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbGetLatestMigrationInfoSession" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public LinqToDbGetLatestMigrationInfoSession(DataConnection dataConnection) : base(dataConnection) { }
    }
}