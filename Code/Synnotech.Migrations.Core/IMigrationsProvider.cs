using System.Collections.Generic;
using System.Reflection;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of retrieving migration instances
    /// according to the latest migration info.
    /// </summary>
    public interface IMigrationsProvider<TMigration, in TMigrationInfo>
    {
        /// <summary>
        /// Gets the list of migrations that should be applied to the target system.
        /// </summary>
        /// <param name="migrationAssembly">The target assembly that is to be searched for migrations.</param>
        /// <param name="latestMigrationInfo">The info of the latest applied migration. Only migrations with a higher version will be returned.</param>
        List<TMigration>? DetermineMigrations(Assembly migrationAssembly, TMigrationInfo? latestMigrationInfo);
    }
}