using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.Linq2Db;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
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
        /// <param name="take">
        /// The amount of migration infos that are retrieved from the database (ordered by AppliedAt). The default value is 100.
        /// The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done because string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="take" /> is equal to or less than 0.</exception>
        public LinqToDbGetLatestMigrationInfoSession(DataConnection dataConnection, int take = 100) : base(dataConnection) =>
            Take = take.MustBeGreaterThan(0, nameof(take));

        private int Take { get; }

        /// <summary>
        /// Gets the latest migration info stored in the target database, or null if no migrations have been
        /// applied. The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done because string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="DbException">Thrown when any communication error with the database occurs.</exception>
        public async Task<TMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var migrationInfos = await DataConnection.GetTable<TMigrationInfo>()
                                                         .OrderBy(migrationInfo => migrationInfo.AppliedAt)
                                                         .Take(Take)
                                                         .ToListAsync(cancellationToken);
                return migrationInfos.GetLatestMigrationInfo();
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
        /// <param name="take">
        /// The amount of migration infos that are retrieved from the database (ordered by AppliedAt). The default value is 50.
        /// The actual latest migration info is determined in-memory using the <see cref="Version" /> object.
        /// This is done because string comparison and <see cref="Version" /> comparison leads to different results
        /// when a slot has more than one digit.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="take" /> is equal or less than 0.</exception>
        public LinqToDbGetLatestMigrationInfoSession(DataConnection dataConnection, int take = 50)
            : base(dataConnection, take) { }
    }
}