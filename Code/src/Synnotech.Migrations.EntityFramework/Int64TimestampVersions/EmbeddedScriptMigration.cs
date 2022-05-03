using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Light.EmbeddedResources;
using Light.GuardClauses;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents a migration that executes a single SQL script against the target
/// database. This script must be an embedded resource that resides within the
/// same namespace as your deriving migration class.
/// </summary>
public abstract class EmbeddedScriptMigration<TDbContext> : Migration<TDbContext>
    where TDbContext : DbContext, IHasMigrationInfos<MigrationInfo>
{
    
    /// <summary>
    /// Initializes a new instance of <see cref="EmbeddedScriptMigration{TDbContext}" />.
    /// </summary>
    /// <param name="scriptName">
    /// The name of the embedded file that contains the SQL script. You must only provide
    /// the name of the file, the namespace will be derived automatically from your deriving class.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scriptName" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="scriptName" /> is an empty string or contains only white space.</exception>
    protected EmbeddedScriptMigration(string scriptName) =>
        ScriptName = scriptName.MustNotBeNullOrWhiteSpace(nameof(scriptName));

    private string ScriptName { get; }

    /// <summary>
    /// Executes the embedded SQL script against the target database.
    /// </summary>
    public sealed override Task ApplyAsync(TDbContext context, CancellationToken cancellationToken = default) =>
        context.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction,
                                                this.GetEmbeddedResource(ScriptName),
                                                cancellationToken);
}