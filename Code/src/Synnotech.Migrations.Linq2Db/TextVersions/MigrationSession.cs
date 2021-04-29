using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default migration session that retrieves and stores migration info instances
    /// via a Linq2Db data connection. You can derive from this type and add all other dependencies that
    /// your migrations need for execution.
    /// </summary>
    /// <typeparam name="TDataConnection">Your database context type that derives from <see cref="DataConnection" />.</typeparam>
    public class MigrationSession<TDataConnection> : MigrationSession<TDataConnection, MigrationInfo>
        where TDataConnection : DataConnection
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession{TDataConnection}" />.
        /// </summary>
        /// <param name="dataConnection">The Linq2Db data connection used to access the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public MigrationSession(TDataConnection dataConnection) : base(dataConnection) { }

        /// <summary>
        /// Gets the latest migration info from the target database. This method
        /// catches exceptions and returns null, so that the migration engine continues
        /// to work even if the target table does not exist.
        /// </summary>
        protected override async Task<MigrationInfo?> GetLatestMigrationInfoAsync()
        {
            try
            {
                var migrationInfos = await DataConnection.GetTable<MigrationInfo>()
                                                         .OrderByDescending(migrationInfo => migrationInfo.AppliedAt)
                                                         .Take(100)
                                                         .ToListAsync();
                return migrationInfos.GetLatestMigrationInfo();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Represents the default migration session that retrieves and stores migration info instances
    /// via a Linq2Db data connection. You can derive from this type and add all other dependencies that
    /// your migrations need for execution.
    /// </summary>
    public class MigrationSession : MigrationSession<DataConnection>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession" />.
        /// </summary>
        /// <param name="dataConnection">The Linq2Db data connection used to access the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataConnection" /> is null.</exception>
        public MigrationSession(DataConnection dataConnection) : base(dataConnection) { }
    }
}