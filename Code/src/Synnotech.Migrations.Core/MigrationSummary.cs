using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the summary of a migration run.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that is stored in the target system to identify which migrations have already been applied.</typeparam>
    public readonly struct MigrationSummary<TMigrationInfo> : IEquatable<MigrationSummary<TMigrationInfo>>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSummary{TMigrationInfo}" /> that indicates that all migrations were applied successfully.
        /// </summary>
        /// <param name="appliedMigrations">The list of migrations that have been applied in this run.</param>
        public MigrationSummary(List<TMigrationInfo>? appliedMigrations)
        {
            AppliedMigrations = appliedMigrations;
            MigrationError = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSummary{TMigrationInfo}" /> that indicates that an error occurred.
        /// </summary>
        /// <param name="migrationError">The error that occurred during migration.</param>
        /// <param name="appliedMigrations">The migrations that were applied before the one where the error occurred.</param>
        public MigrationSummary(IMigrationError migrationError, List<TMigrationInfo>? appliedMigrations)
        {
            MigrationError = migrationError;
            AppliedMigrations = appliedMigrations;
        }

        /// <summary>
        /// Gets an empty instance, indicating that no migrations were applied.
        /// </summary>
        public static MigrationSummary<TMigrationInfo> Empty => new ();

        /// <summary>
        /// Gets the collection of migrations that were applied.
        /// </summary>
        private List<TMigrationInfo>? AppliedMigrations { get; }

        /// <summary>
        /// Gets a possible migration error that occurred during the migration.
        /// </summary>
        private IMigrationError? MigrationError { get; }

        /// <summary>
        /// Tries to get the collection of applied migrations.
        /// </summary>
        /// <param name="appliedMigrations">The migrations that were applied in a migration run.</param>
        /// <returns>True if migrations were applied, else false.</returns>
        public bool TryGetAppliedMigrations([NotNullWhen(true)] out List<TMigrationInfo>? appliedMigrations)
        {
            if (AppliedMigrations.IsNullOrEmpty())
            {
                appliedMigrations = null;
                return false;
            }

            appliedMigrations = AppliedMigrations;
            return true;
        }

        /// <summary>
        /// Tries to get the error that occurred during migration.
        /// </summary>
        /// <param name="migrationError">The error that occurred during migration.</param>
        public bool TryGetError([NotNullWhen(true)] out IMigrationError? migrationError)
        {
            if (MigrationError != null)
            {
                migrationError = MigrationError;
                return true;
            }

            migrationError = null;
            return false;
        }

        /// <summary>
        /// Ensures that no errors occurred during the migration, or otherwise throws a <see cref="MigrationException" />.
        /// </summary>
        /// <exception cref="MigrationException">Thrown when this summary contains a <see cref="MigrationError" />.</exception>
        public void EnsureSuccess() => MigrationError?.Rethrow();

        /// <summary>
        /// Checks if the specified migration summary is equal to this instance.
        /// This is true when the migration error and all applied migrations are equal.
        /// </summary>
        public bool Equals(MigrationSummary<TMigrationInfo> other)
        {
            if (!(MigrationError?.Equals(other.MigrationError) ?? other.MigrationError is null))
                return false;
            if (AppliedMigrations?.Count != other.AppliedMigrations?.Count || AppliedMigrations.IsNullOrEmpty())
                return false;

            for (var i = 0; i < AppliedMigrations.Count; i++)
            {
                if (!EqualityComparer<TMigrationInfo>.Default.Equals(AppliedMigrations[i], other.AppliedMigrations![i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the specified object is a migration summary and equal to this instance.
        /// This is true when the migration error and all applied migrations are equal.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is MigrationSummary<TMigrationInfo> summary && Equals(summary);

        /// <summary>
        /// Gets the hash code for this migration summary.
        /// </summary>
        public override int GetHashCode()
        {
            if (AppliedMigrations.IsNullOrEmpty())
                return MigrationError?.GetHashCode() ?? 0;

            var builder = MultiplyAddHashBuilder.Create();
            builder.CombineIntoHash(MigrationError);

            for (var i = 0; i < AppliedMigrations.Count; i++)
            {
                builder.CombineIntoHash(AppliedMigrations[i]);
            }

            return builder.BuildHash();
        }

        /// <summary>
        /// Returns the string representation of this migration summary.
        /// </summary>
        public override string ToString() =>
            MigrationError != null ?
                "Error occurred at migration " + MigrationError.ErroneousVersionText :
                !AppliedMigrations.IsNullOrEmpty() ?
                    "Migrations applied" :
                    "No migrations applied";
    }
}