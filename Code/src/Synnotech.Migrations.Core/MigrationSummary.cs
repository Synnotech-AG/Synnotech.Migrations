using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the summary of a migration run.
    /// </summary>
    /// <typeparam name="TMigrationInfo">The type that is stored in the target system to identify which migrations have already been applied.</typeparam>
    public readonly struct MigrationSummary<TMigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSummary{TMigrationInfo}" /> that indicates that migrations were applied successfully.
        /// </summary>
        /// <param name="appliedMigrations">The list of migrations that have been applied in this run.</param>
        public MigrationSummary(List<TMigrationInfo> appliedMigrations)
        {
            AppliedMigrations = appliedMigrations;
            MigrationError = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MigrationSummary{TMigrationInfo}" /> that indicates that an error occurred.
        /// </summary>
        /// <param name="migrationError">The error that occurred during migration.</param>
        /// <param name="appliedMigrations">The migrations that were applied before the one where the error occurred.</param>
        public MigrationSummary(MigrationError<TMigrationInfo> migrationError, List<TMigrationInfo>? appliedMigrations)
        {
            MigrationError = migrationError.MustNotBeNull(nameof(migrationError));
            AppliedMigrations = appliedMigrations;
        }

        /// <summary>
        /// Gets an empty instance, indicating that no migrations were applied.
        /// </summary>
        public static MigrationSummary<TMigrationInfo> Empty => new MigrationSummary<TMigrationInfo>();

        /// <summary>
        /// Gets the collection of migrations that were applied.
        /// </summary>
        public List<TMigrationInfo>? AppliedMigrations { get; }

        /// <summary>
        /// Gets a possible migration error that occurred during the migration.
        /// </summary>
        public MigrationError<TMigrationInfo>? MigrationError { get; }

        /// <summary>
        /// Tries to get the collection of applied migrations.
        /// </summary>
        /// <param name="appliedMigrations">The migrations that were applied in a migration run.</param>
        /// <returns>True if migrations were applied, else false.</returns>
        public bool TryGetAppliedMigrations([NotNullWhen(true)] out List<TMigrationInfo>? appliedMigrations)
        {
            if (AppliedMigrations.IsNullOrEmpty())
            {
                appliedMigrations = default;
                return false;
            }

            appliedMigrations = AppliedMigrations;
            return true;
        }

        /// <summary>
        /// Tries to get the error that occurred during migration.
        /// </summary>
        /// <param name="migrationError">The error that occurred during migration.</param>
        public bool TryGetError([NotNullWhen(true)] out MigrationError<TMigrationInfo>? migrationError)
        {
            if (MigrationError == null)
            {
                migrationError = null;
                return false;
            }

            migrationError = MigrationError;
            return true;
        }

        /// <summary>
        /// Ensures that no errors occurred during the migration, or otherwise throws a <see cref="MigrationException" />.
        /// </summary>
        /// <exception cref="MigrationException">Thrown when this summary contains a <see cref="MigrationError" />.</exception>
        public void EnsureSuccess() => MigrationError?.Rethrow();
    }
}