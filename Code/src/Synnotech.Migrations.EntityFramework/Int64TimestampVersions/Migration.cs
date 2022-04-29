using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents the default abstraction of a migration applied via EntityFramework.
/// This type uses <see cref="long" /> values to identify and sort migrations.
/// <typeparam name="TDbContext">The type of the database context. It must derive from <see cref="DbContext" />.</typeparam>
/// </summary>
public abstract class Migration<TDbContext> : BaseMigration, IMigration<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="Migration{TDbContext}" />.
    /// The version is retrieved via the <see cref="MigrationVersionAttribute" />.
    /// </summary>
    /// <param name="name">
    /// The name of the migration (optional). If the string is null, empty, or contains only white space,
    /// then the simple type name (not the fully-qualified name) is used.
    /// </param>
    /// <exception cref="InvalidOperationException">Thrown when the deriving class is not decorated with the <see cref="MigrationVersionAttribute" />.</exception>
    protected Migration(string? name = null) : base(name) { }

    /// <summary>
    /// Gets the value indicating whether this migration requires a transaction.
    /// The default value is true.
    /// Deriving classes can set this value.
    /// </summary>
    public bool IsRequiringTransaction { get; protected set; } = true;

    /// <summary>
    /// Executes the migration. Interactions with the target system can be performed
    /// using the specified database context.
    /// IMPORTANT: you usually should not call 'dbContext.SaveChangesAsync' or something
    /// similar as this is handled by the migration engine.
    /// </summary>
    /// <param name="dbContext">>The EntityFramework database context that is used to interact with the target database.</param>
    /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
    public abstract Task ApplyAsync(TDbContext dbContext, CancellationToken cancellationToken = default);
}