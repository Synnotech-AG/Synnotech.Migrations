using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a plan about the migrations that the engine will apply.
    /// </summary>
    /// <typeparam name="TMigration">The type that represents the general abstraction for migrations.</typeparam>
    /// <typeparam name="TMigrationInfo">
    /// The type that is stored in the target system to identify which migrations have already been applied.
    /// </typeparam>
    public readonly struct MigrationPlan<TMigration, TMigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationPlan{TMigration, TMigrationInfo}"/>.
        /// </summary>
        /// <param name="currentVersion">The version that is currently applied to the target database.</param>
        /// <param name="migrationsToBeApplied">The list of migrations that will be applied by the engine.</param>
        public MigrationPlan(TMigrationInfo? currentVersion,
                             List<TMigration>? migrationsToBeApplied)
        {
            CurrentVersion = currentVersion;
            MigrationsToBeApplied = migrationsToBeApplied;
        }

        /// <summary>
        /// Gets the version that is currently applied to the target database.
        /// </summary>
        public TMigrationInfo? CurrentVersion { get; }

        /// <summary>
        /// Gets the list of migrations that will be applied by the engine.
        /// </summary>
        public List<TMigration>? MigrationsToBeApplied { get; }

        /// <summary>
        /// Gets the value indicating whether there are pending migrations.
        /// </summary>
        public bool HasPendingMigrations => !MigrationsToBeApplied.IsNullOrEmpty();
    }
}