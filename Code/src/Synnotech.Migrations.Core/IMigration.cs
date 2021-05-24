using System.Threading.Tasks;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a migration that accesses the target system in an async fashion.
    /// All interactions with the target system are performed using the context. You
    /// can customize what is available to you by supplying a corresponding type
    /// to the generic <typeparamref name="TContext" />.
    /// IMPORTANT: you usually should not call 'context.SaveChangesAsync' or something
    /// similar as this is handled by the migration engine.
    /// If possible, throw a <see cref="MigrationException" /> to indicate errors that
    /// occurred during the migration.
    /// </summary>
    /// <typeparam name="TContext">The type that represents the context for interactions with the target system.</typeparam>
    public interface IMigration<in TContext>
    {
        /// <summary>
        /// Executes the migration. Interactions with the target system can be performed
        /// using the specified context.
        /// IMPORTANT: you usually should not call 'context.SaveChangesAsync' or something
        /// similar as this is handled by the migration engine.
        /// If possible, throw a <see cref="MigrationException" /> to indicate errors that
        /// occurred during the migration.
        /// </summary>
        /// <param name="context">The context that holds all structures necessary to perform the migration.</param>
        Task ApplyAsync(TContext context);
    }
}