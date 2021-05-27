using System.Threading;
using System.Threading.Tasks;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of a session to the target system that is used
    /// when a migration is applied. The context will be passed to the migration.
    /// When the migration was applied successfully, a migration info will be created
    /// and stored in the target system. At the end, SaveChanges will be called to
    /// permanently store all changes / commit the transaction.
    /// </summary>
    /// <typeparam name="TContext">The context that is passed to a migration being applied.</typeparam>
    /// <typeparam name="TMigrationInfo">
    /// That type whose instances are stored in the target system to indicate which
    /// migrations already have been applied.
    /// </typeparam>
    public interface IMigrationSession<out TContext, in TMigrationInfo> : IAsyncSession
    {
        /// <summary>
        /// Gets the context that will be passed to a migration.
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// Stores the specified migration info in the target system.
        /// </summary>
        /// <param name="migrationInfo">The migration info object that will be stored in the database.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        ValueTask StoreMigrationInfoAsync(TMigrationInfo migrationInfo, CancellationToken cancellationToken = default);
    }
}