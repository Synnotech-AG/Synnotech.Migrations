using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents the session that is used to apply a single migration and store a corresponding migration info via LinqToDB.
/// </summary>
/// <typeparam name="TDbContext">Your custom DbContext that derives from <see cref="DbContext" />.</typeparam>
/// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
public class EntityFrameworkMigrationSession<TDbContext, TMigrationInfo> : IMigrationSession<TDbContext, TMigrationInfo>
    where TDbContext : DbContext, IHasMigrationInfos<TMigrationInfo>
    where TMigrationInfo : BaseMigrationInfo
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityFrameworkMigrationSession{TDbContext,TMigrationInfo}" />.
    /// </summary>
    /// <param name="context">The EntityFramework database context used to interact with the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    public EntityFrameworkMigrationSession(TDbContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Returns the underlying EntityFramework database context that is used as the context for migrations.
    /// </summary>
    public TDbContext Context { get; }

    /// <inheritdoc />
    public void Dispose() => Context.Dispose();

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Context.Dispose();
        return default;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken);
        Context.Database.CurrentTransaction?.Commit();
    }

    /// <summary>
    /// Inserts the specified migration info into the database. If the database is configured to generate
    /// an ID for the migration info when this statement is executed, it will not be written back to the
    /// migration info entity.
    /// </summary>
    public virtual ValueTask StoreMigrationInfoAsync(TMigrationInfo migrationInfo, CancellationToken cancellationToken = default)
    {
        Context.MigrationInfos.Add(migrationInfo);
        return default;
    }
}

/// <summary>
/// Represents the session that is used to apply migrations and store corresponding migration info via EntityFramework.
/// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
/// </summary>
/// <typeparam name="TDbContext">Your custom subtype that derives from <see cref="DbContext" />.</typeparam>
public class EntityFrameworkMigrationSession<TDbContext> : EntityFrameworkMigrationSession<TDbContext, MigrationInfo>
    where TDbContext : DbContext, IHasMigrationInfos<MigrationInfo>
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityFrameworkMigrationSession{TDbContext}" />.
    /// </summary>
    /// <param name="context">>The EntityFramework database context used to interact with the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context" /> is null.</exception>
    public EntityFrameworkMigrationSession(TDbContext context) : base(context) { }
}