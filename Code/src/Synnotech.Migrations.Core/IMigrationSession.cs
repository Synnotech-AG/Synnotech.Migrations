using System.Threading.Tasks;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the interactions with the target system that the migration engine performs
    /// to keep track which migrations have already been applied and which should be applied.
    /// </summary>
    /// <typeparam name="TMigrationInfo">
    /// The type that represents metadata information about an applied migration.
    /// This is usually stored in a single document or table. The engine can then
    /// simply load the latest migration info to determine if there are any migrations
    /// that need to be applied to the target system.
    /// </typeparam>
    public interface IMigrationSession<TMigrationInfo> : IAsyncSession
    {
        /// <summary>
        /// Gets the metadata about the latest applied migration (i.e. the MigrationInfo with the highest version)
        /// that is stored in the target system. This method can return null if no migrations have been
        /// applied to the target system.
        /// </summary>
        Task<TMigrationInfo?> GetLatestMigrationInfoAsync();

        /// <summary>
        /// Stores metadata about a migration that has just been applied to the database.
        /// </summary>
        Task StoreMigrationInfoAsync(TMigrationInfo migrationInfo);
    }
}