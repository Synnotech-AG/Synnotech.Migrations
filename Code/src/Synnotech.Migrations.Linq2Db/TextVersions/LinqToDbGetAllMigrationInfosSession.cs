using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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
    /// Represents the session that retrieves all migration infos for LinqToDB.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
    public class LinqToDbGetAllMigrationInfosSession<TMigrationInfo> : AsyncReadOnlySession, IGetAllMigrationInfosSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbGetAllMigrationInfosSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection"/> is null.</exception>
        public LinqToDbGetAllMigrationInfosSession(DataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Gets all migration infos stored in the target database.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public async Task<List<TMigrationInfo>> GetAllMigrationInfosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var migrationInfos = await DataConnection.GetTable<TMigrationInfo>()
                                                         .OrderBy(migrationInfo => migrationInfo.AppliedAt)
                                                         .ToListAsync(cancellationToken);
                return migrationInfos;
            }
            catch
            {
                return new List<TMigrationInfo>(0);
            }
        }
    }

    /// <summary>
    /// Represents the session that retrieves all migration infos for LinqToDB.
    /// <see cref="MigrationInfo" /> is used as the the type that represents stored migration infos.
    /// </summary>
    public sealed class LinqToDbGetAllMigrationInfosSession : LinqToDbGetAllMigrationInfosSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LinqToDbGetAllMigrationInfosSession" />.
        /// </summary>
        /// <param name="dataConnection">The LinqToDB data connection used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection"/> is null.</exception>
        public LinqToDbGetAllMigrationInfosSession(DataConnection dataConnection) : base(dataConnection) { }
    }
}