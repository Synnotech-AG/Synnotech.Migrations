using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of a factory that creates different session instances for the migration engine.
    /// These sessions are used to communicate with the target system to retrieve or store data.
    /// </summary>
    /// <typeparam name="TMigrationInfo">
    /// That type whose instances are stored in the target system to indicate which
    /// migrations already have been applied.
    /// </typeparam>
    /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
    /// <typeparam name="TContext">The type whose instances are passed to each migration when they are applied.</typeparam>
    public interface ISessionFactory<TMigrationInfo, in TMigration, TContext>
    {
        /// <summary>
        /// Creates a new session that will be used to retrieve the latest applied migration info.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        ValueTask<IGetLatestMigrationInfoSession<TMigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new session that will be used to execute the specified migration.
        /// </summary>
        /// <param name="migration">The migration to be executed.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="migration" /> is null.</exception>
        ValueTask<IMigrationSession<TContext, TMigrationInfo>> CreateSessionForMigrationAsync(TMigration migration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new session that will be used to retrieve all migration infos.
        /// </summary>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        ValueTask<IGetAllMigrationInfosSession<TMigrationInfo>> CreateSessionForRetrievingAllMigrationInfosAsync(CancellationToken cancellationToken = default);
    }
}