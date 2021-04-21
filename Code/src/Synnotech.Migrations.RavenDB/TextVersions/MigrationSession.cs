using System;
using System.Threading.Tasks;
using Light.GuardClauses;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Synnotech.DatabaseAbstractions;
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
    public class MigrationSession<TMigrationInfo> : IMigrationSession<TMigrationInfo>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSession{TMigrationInfo}" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <param name="waitForIndexesAfterSaveChanges">The value indicating whether the RavenDB client waits for all indexes to be updated on SaveChangesAsync.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        public MigrationSession(IAsyncDocumentSession session, bool waitForIndexesAfterSaveChanges = true)
        {
            Session = session.MustNotBeNull(nameof(session));
            if (waitForIndexesAfterSaveChanges)
                Session.Advanced.WaitForIndexesAfterSaveChanges();
        }

        Task<TMigrationInfo?> IMigrationSession<TMigrationInfo>.GetLatestMigrationInfoAsync() =>
#nullable disable
            Session.Query<TMigrationInfo>()
                   .OrderByDescending(migrationInfo => migrationInfo.Version)
                   .FirstOrDefaultAsync();
#nullable restore

        Task IMigrationSession<TMigrationInfo>.StoreMigrationInfoAsync(TMigrationInfo migrationInfo) =>
            Session.StoreAsync(migrationInfo, "migrationInfos" + Session.Advanced.DocumentStore.Conventions.IdentityPartsSeparator + migrationInfo.Version);

        Task IAsyncSession.SaveChangesAsync() => Session.SaveChangesAsync();

        void IDisposable.Dispose() => Session.Dispose();

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            Session.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        /// <summary>
        /// Gets the session object used to query or update the target database.
        /// </summary>
        public IAsyncDocumentSession Session { get; }
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
        /// <param name="waitForIndexesAfterSaveChanges">The value indicating whether the RavenDB client waits for all indexes to be updated on SaveChangesAsync.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        public MigrationSession(IAsyncDocumentSession session, bool waitForIndexesAfterSaveChanges = true) : base(session, waitForIndexesAfterSaveChanges) { }
    }
}