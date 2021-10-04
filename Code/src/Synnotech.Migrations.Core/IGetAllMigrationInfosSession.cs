using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of a session that can retrieve all infos about applied migrations.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that represents a migration info.</typeparam>
    public interface IGetAllMigrationInfosSession<TMigrationInfo> : IAsyncReadOnlySession
    {
        /// <summary>
        /// Gets all migration infos stored in the target system. This method might return an empty list.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        Task<List<TMigrationInfo>> GetAllMigrationInfosAsync(CancellationToken cancellationToken = default);
    }
}