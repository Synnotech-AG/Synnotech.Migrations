using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Synnotech.EntityFramework;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents the session that retrieves all migration infos for EntityFramework.
/// </summary>
/// <typeparam name="TMigrationInfo">The type that represents a migration info. It must derive from <see cref="BaseMigrationInfo" />.</typeparam>
/// <typeparam name="TDbContext">The type of the database context. It must derive from <see cref="DbContext" /> and implement <see cref="IHasMigrationInfos{TMigrationInfo}" />.</typeparam>
public class EntityFrameworkGetAllMigrationInfosSession<TDbContext, TMigrationInfo> : AsyncReadOnlySession<TDbContext>, IGetAllMigrationInfosSession<TMigrationInfo>
    where TDbContext : DbContext, IHasMigrationInfos<TMigrationInfo>
    where TMigrationInfo : BaseMigrationInfo
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityFrameworkGetAllMigrationInfosSession{TDbContext, TMigrationInfo}" />.
    /// </summary>
    /// <param name="dbContext">The EntityFramework database context used to interact with the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    public EntityFrameworkGetAllMigrationInfosSession(TDbContext dbContext) : base(dbContext) { }

    /// <summary>
    /// Gets all migration infos stored in the target database.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
    public async Task<List<TMigrationInfo>> GetAllMigrationInfosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.MigrationInfos
                                .OrderByDescending(migrationInfo => migrationInfo.Version)
                                .ToListAsync(cancellationToken);
        }
        catch
        {
            return new List<TMigrationInfo>(0);
        }
    }
}

/// <summary>
/// Represents the session that retrieves all migration infos for EntityFramework.
/// <see cref="MigrationInfo" /> is used as the the type that represents stored migration infos.
/// </summary>
public sealed class EntityFrameworkGetAllMigrationInfosSession<TDbContext> : EntityFrameworkGetAllMigrationInfosSession<TDbContext, MigrationInfo>
    where TDbContext : DbContext, IHasMigrationInfos<MigrationInfo>
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityFrameworkGetAllMigrationInfosSession{TDbContext,TMigrationInfo}" />.
    /// </summary>
    /// <param name="dbContext">The EntityFramework database context used to interact with the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    public EntityFrameworkGetAllMigrationInfosSession(TDbContext dbContext) : base(dbContext) { }
}