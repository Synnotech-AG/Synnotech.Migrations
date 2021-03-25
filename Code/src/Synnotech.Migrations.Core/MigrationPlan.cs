using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a plan that contains the current version of the target system and the pending migrations that need to be applied to it.
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
        /// <param name="currentVersion">The version that is currently applied to the target system.</param>
        /// <param name="pendingMigrations">The list of migrations that need to be applied to the target system.</param>
        public MigrationPlan(TMigrationInfo? currentVersion,
                             List<TMigration>? pendingMigrations)
        {
            CurrentVersion = currentVersion;
            PendingMigrations = pendingMigrations;
        }

        /// <summary>
        /// Gets the version that is currently applied to the target database.
        /// </summary>
        public TMigrationInfo? CurrentVersion { get; }

        /// <summary>
        /// Gets the list of migrations that will be applied by the engine.
        /// </summary>
        public List<TMigration>? PendingMigrations { get; }

        /// <summary>
        /// Gets the value indicating whether there are pending migrations.
        /// </summary>
        public bool HasPendingMigrations => !PendingMigrations.IsNullOrEmpty();

        /// <summary>
        /// Checks if the other migration plan is equal to this instance. For this to be true, the current version as well
        /// as each migration to be applied must be equal in both plans.
        /// </summary>
        public bool Equals(MigrationPlan<TMigration, TMigrationInfo> other)
        {
            if (!EqualityComparer<TMigrationInfo?>.Default.Equals(CurrentVersion, other.CurrentVersion))
                return false;
            if (PendingMigrations?.Count != other.PendingMigrations?.Count || PendingMigrations.IsNullOrEmpty())
                return false;

            for (var i = 0; i < PendingMigrations.Count; i++)
            {
                if (!EqualityComparer<TMigration>.Default.Equals(PendingMigrations[i], other.PendingMigrations![i]))
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
            if (PendingMigrations.IsNullOrEmpty())
                return CurrentVersion?.GetHashCode() ?? 0;

            var builder = MultiplyAddHashBuilder.Create();
            builder.CombineIntoHash(CurrentVersion);

            for (var i = 0; i < PendingMigrations.Count; i++)
            {
                builder.CombineIntoHash(PendingMigrations[i]);
            }

            return builder.BuildHash();
        }

        /// <summary>
        /// Returns the string representation of this migration plan.
        /// </summary>
        public override string ToString() => HasPendingMigrations ? "Pending migrations" : "No pending migrations";
    }
}