using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default migration session that retrieves and stores migration info instances
    /// in the RavenDB document database. You can derive from this type and add all other dependencies that
    /// your migrations need for execution.
    /// IMPORTANT: you should not call SaveChangesAsync in your migrations, as this is done by the migration engine.
    /// </summary>
    public class MigrationSession<TMigrationInfo> : AsyncSession, IAsyncMigrationSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <param name="waitForIndexesAfterSaveChanges">The value indicating whether the RavenDB client waits for all indexes to be updated during a <see cref="AsyncSession.SaveChangesAsync" /> call.</param>
        public MigrationSession(IAsyncDocumentSession session, bool waitForIndexesAfterSaveChanges = true) : base(session, waitForIndexesAfterSaveChanges) { }

        Task<TMigrationInfo?> IAsyncMigrationSession<TMigrationInfo>.GetLatestMigrationInfoAsync() =>
#nullable disable
            Session.Query<TMigrationInfo>()
                   .OrderByDescending(migrationInfo => migrationInfo!.Version)
                   .FirstOrDefaultAsync();
#nullable restore

        Task IAsyncMigrationSession<TMigrationInfo>.StoreMigrationInfoAsync(TMigrationInfo migrationInfo) =>
            Session.StoreAsync(migrationInfo, "migrationInfos" + Session.Advanced.DocumentStore.Conventions.IdentityPartsSeparator + migrationInfo.Version);
    }

    /// <summary>
    /// Represents the default migration session that retrieves and stores <see cref="MigrationInfo" /> instances
    /// in the RavenDB document database. You can derive from this type and add all other dependencies that
    /// your migrations need for execution.
    /// IMPORTANT: you should not call SaveChangesAsync in your migrations, as this is done by the migration engine.
    /// </summary>
    public class MigrationSession : MigrationSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <param name="waitForIndexesAfterSaveChanges">The value indicating whether the RavenDB client waits for all indexes to be updated during a <see cref="AsyncSession.SaveChangesAsync" /> call.</param>
        public MigrationSession(IAsyncDocumentSession session, bool waitForIndexesAfterSaveChanges = true) : base(session, waitForIndexesAfterSaveChanges) { }
    }
}