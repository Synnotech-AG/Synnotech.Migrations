using Light.EmbeddedResources;
using Light.GuardClauses;
using System;
using System.Data;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents a migration that executes several SQL scripts against the target
/// database. The scripts must all be embedded resources that reside within the same
/// namespace as your deriving migration class.
/// </summary>
public abstract class EmbeddedScriptsMigration<TDbContext> : Migration<TDbContext>
    where TDbContext : DbContext, IHasMigrationInfoTable<MigrationInfo>
{
    /// <summary>
    /// Initializes a new instance of <see cref="EmbeddedScriptsMigration{TDbContext}" />
    /// </summary>
    /// <param name="scriptNames">The names of the scripts that will be executed. The order matters (of course!).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scriptNames" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="scriptNames" /> is empty.</exception>
    protected EmbeddedScriptsMigration(params string[] scriptNames) =>
        ScriptNames = scriptNames.MustNotBeNullOrEmpty(nameof(scriptNames));

    private string[] ScriptNames { get; }

    /// <summary>
    /// Executes the embedded SQL scripts against the target database.
    /// </summary>
    public sealed override async Task ApplyAsync(TDbContext context, CancellationToken cancellationToken = default)
    {
        foreach (var scriptName in ScriptNames)
        {
            await context.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction,
                                                          this.GetEmbeddedResource(scriptName),
                                                          cancellationToken);
        }
    }
}