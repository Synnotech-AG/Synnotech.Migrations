using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a plan about the migrations that the engine will apply.
    /// </summary>
    /// <typeparam name="TMigration">The type that represents the general abstraction for migrations.</typeparam>
    /// <typeparam name="TMigrationInfo">
    /// The type that is stored in the target system to identify which migrations have already been applied.
    /// </typeparam>
    public readonly struct MigrationPlan<TMigration, TMigrationInfo> : IEquatable<MigrationPlan<TMigration, TMigrationInfo>>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationPlan{TMigration, TMigrationInfo}" />.
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

        /// <summary>
        /// Checks if the other migration plan is equal to this instance. For this to be true, the current version as well
        /// as each migration to be applied must be equal in both plans.
        /// </summary>
        public bool Equals(MigrationPlan<TMigration, TMigrationInfo> other)
        {
            if (!EqualityComparer<TMigrationInfo?>.Default.Equals(CurrentVersion, other.CurrentVersion))
                return false;
            if (MigrationsToBeApplied?.Count != other.MigrationsToBeApplied?.Count || MigrationsToBeApplied.IsNullOrEmpty())
                return false;

            for (var i = 0; i < MigrationsToBeApplied.Count; i++)
            {
                if (!EqualityComparer<TMigration>.Default.Equals(MigrationsToBeApplied[i], other.MigrationsToBeApplied![i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the specified object is a migration plan and if it has the same values as this instance.
        /// For this to be true, the current version as well as each migration to be applied must be equal in both plans.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is MigrationPlan<TMigration, TMigrationInfo> plan && Equals(plan);

        /// <summary>
        /// Gets the hash code of this migration plan.
        /// </summary>
        public override int GetHashCode()
        {
            if (MigrationsToBeApplied.IsNullOrEmpty())
                return CurrentVersion?.GetHashCode() ?? 0;

            var builder = MultiplyAddHashBuilder.Create();
            builder.CombineIntoHash(CurrentVersion);

            for (var i = 0; i < MigrationsToBeApplied.Count; i++)
            {
                builder.CombineIntoHash(MigrationsToBeApplied[i]);
            }

            return builder.BuildHash();
        }
    }
}