using System;
using System.Data;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using Synnotech.Migrations.Core;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents the default factory for creating EntityFramework migration sessions that support text versions.
/// </summary>
public sealed class SessionFactory<TDbContext> : ISessionFactory<MigrationInfo, Migration<TDbContext>, TDbContext>
    where TDbContext : DbContext, IHasMigrationInfos<MigrationInfo>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SessionFactory{TDbContext}" />.
    /// </summary>
    /// <param name="createDbContext">The factory that creates a new EntityFramework DbContext.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="createDbContext" /> is null.</exception>
    public SessionFactory(Func<TDbContext> createDbContext) =>
        CreateDbContext = createDbContext.MustNotBeNull(nameof(createDbContext));

    private Func<TDbContext> CreateDbContext { get; }

    /// <summary>
    /// Creates the session that is used to retrieve the latest migration info from the target database.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
    public ValueTask<IGetLatestMigrationInfoSession<MigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken = default) =>
        new(new EntityFrameworkGetLatestMigrationInfoSession<TDbContext, MigrationInfo>(CreateDbContext()));

    /// <summary>
    /// Creates the session that is used to apply a migration and store the corresponding migration info in the target database.
    /// </summary>
    /// <param name="migration">
    /// The migration to be executed. <see cref="Migration{TDbContext}.IsRequiringTransaction" /> is used to determine if a transaction
    /// is started on the database context.
    /// </param>
    /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="migration" /> is null.</exception>
    public ValueTask<IMigrationSession<TDbContext, MigrationInfo>> CreateSessionForMigrationAsync(Migration<TDbContext> migration,
                                                                                                  CancellationToken cancellationToken = default)
    {
        migration.MustNotBeNull(nameof(migration));

        var dbContext = CreateDbContext();
        if (migration.IsRequiringTransaction)
            dbContext.Database.BeginTransaction(IsolationLevel.Serializable);
        return new(new EntityFrameworkMigrationSession<TDbContext>(dbContext));
    }

    /// <summary>
    /// Creates the session that is used to retrieve all migration infos from the target database.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
    public ValueTask<IGetAllMigrationInfosSession<MigrationInfo>> CreateSessionForRetrievingAllMigrationInfosAsync(CancellationToken cancellationToken = default) =>
        new(new EntityFrameworkGetAllMigrationInfosSession<TDbContext, MigrationInfo>(CreateDbContext()));
}