using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a plan that contains the current version of the target system and the pending migrations that need to be applied to it.
    /// </summary>
    /// <typeparam name="TMigrationVersion">The type that represents a migration version.</typeparam>
    /// <typeparam name="TMigrationInfo">
    /// The type that is stored in the target system to identify which migrations have already been applied.
    /// </typeparam>
    public readonly struct MigrationPlan<TMigrationVersion, TMigrationInfo> : IEquatable<MigrationPlan<TMigrationVersion, TMigrationInfo>>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        where TMigrationInfo : IHasMigrationVersion<TMigrationVersion>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationPlan{TMigrationVersion, TMigrationInfo}" />.
        /// </summary>
        /// <param name="currentVersionInfo">The migration info indicating the version that is currently applied to the target system.</param>
        /// <param name="pendingMigrations">The list of migrations that need to be applied to the target system.</param>
        public MigrationPlan(TMigrationInfo? currentVersionInfo, List<PendingMigration<TMigrationVersion>>? pendingMigrations)
        {
            CurrentVersionInfo = currentVersionInfo;
            PendingMigrations = pendingMigrations;
        }

        /// <summary>
        /// Gets the version that is currently applied to the target database.
        /// </summary>
        public TMigrationInfo? CurrentVersionInfo { get; }

        /// <summary>
        /// Gets the list of pending migrations.
        /// </summary>
        public List<PendingMigration<TMigrationVersion>>? PendingMigrations { get; }

        /// <summary>
        /// Gets the value indicating whether there are pending migrations.
        /// </summary>
        public bool HasPendingMigrations => !PendingMigrations.IsNullOrEmpty();

        /// <summary>
        /// Checks if the other migration plan is equal to this instance. For this to be true, the current version info
        /// as well as each migration to be applied must be equal in both plans.
        /// </summary>
        public bool Equals(MigrationPlan<TMigrationVersion, TMigrationInfo> other)
        {
            if (!CheckIfCurrentVersionsAreEqual(CurrentVersionInfo, other.CurrentVersionInfo))
                return false;
            if (PendingMigrations?.Count != other.PendingMigrations?.Count || PendingMigrations.IsNullOrEmpty())
                return false;

            for (var i = 0; i < PendingMigrations.Count; i++)
            {
                var thisMigration = PendingMigrations[i];
                var otherMigration = other.PendingMigrations![i];

                if (thisMigration != otherMigration)
                    return false;
            }

            return true;

            static bool CheckIfCurrentVersionsAreEqual(TMigrationInfo? x, TMigrationInfo? y) =>
            x is null ?
                y is null :
                y is not null && x.GetMigrationVersion().Equals(y.GetMigrationVersion());
        }

        /// <summary>
        /// Checks if the specified object is a migration plan and if it has the same values as this instance.
        /// For this to be true, the current version info as well as each migration to be applied must be equal in both plans.
        /// </summary>
        public override bool Equals(object obj) => obj is MigrationPlan<TMigrationVersion, TMigrationInfo> migrationPlan && Equals(migrationPlan);

        /// <summary>
        /// Gets the hash code of this migration plan.
        /// </summary>
        public override int GetHashCode()
        {
            if (PendingMigrations.IsNullOrEmpty())
                return CurrentVersionInfo?.GetHashCode() ?? 0;

            var builder = MultiplyAddHashBuilder.Create();
            builder.CombineIntoHash(CurrentVersionInfo);

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