using System.Threading.Tasks;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents an abstraction of the session that is used to retrieve the
    /// latest migration info.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info.</typeparam>
    public interface IGetLatestMigrationInfoSession<TMigrationInfo> : IAsyncReadOnlySession
    {
        /// <summary>
        /// Gets the latest migration info stored in the target system, or null if no
        /// migration has been applied.
        /// </summary>
        Task<TMigrationInfo?> GetLatestMigrationInfoAsync();
    }
}