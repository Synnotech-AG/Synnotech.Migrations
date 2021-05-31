using System;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Captures an error that occurred during a migration.
    /// </summary>
    public sealed class MigrationError<TMigrationVersion> : IMigrationError, IEquatable<MigrationError<TMigrationVersion>>
        where TMigrationVersion : IEquatable<TMigrationVersion>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationError{TMigrationVersion}" />.
        /// </summary>
        /// <param name="versionWithError">The version of the migration that could not be executed.</param>
        /// <param name="exception">The exception that occurred during the migration.</param>
        public MigrationError(TMigrationVersion versionWithError, Exception exception)
        {
            ErroneousVersion = versionWithError.MustNotBeNullReference(nameof(versionWithError));
            Exception = exception.MustNotBeNull(nameof(exception));
        }

        /// <summary>
        /// Gets the version of the erroneous migration.
        /// </summary>
        public TMigrationVersion ErroneousVersion { get; }

        /// <summary>
        /// Checks if the specified object is a migration error and equal to this instance.
        /// This is true when the version represents the same migration as the other instance.
        /// </summary>
        public bool Equals(MigrationError<TMigrationVersion>? other) =>
            other is not null && ErroneousVersion.Equals(other.ErroneousVersion);

        /// <summary>
        /// Gets the erroneous version as a string.
        /// </summary>
        public string ErroneousVersionText => ErroneousVersion.ToString();

        /// <summary>
        /// Gets the exception that was thrown when the migration was executed.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Throws a new <see cref="MigrationException" /> containing the actual <see cref="Exception" /> as an inner exception.
        /// </summary>
        /// <exception cref="MigrationException">Always thrown</exception>
        [DoesNotReturn]
        public void Rethrow() =>
            throw new MigrationException($"Could not execute migration \"{ErroneousVersion}\" successfully - please see the inner exception for details", Exception);

        /// <summary>
        /// Checks if the specified object is a migration error and equal to this instance.
        /// This is true when the version represents the same migration as the other instance.
        /// </summary>
        public bool Equals(IMigrationError? migrationError) =>
            migrationError is MigrationError<TMigrationVersion> error && Equals(error);

        /// <summary>
        /// Gets the hash code of this migration error.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is MigrationError<TMigrationVersion> error && Equals(error);

        /// <summary>
        /// Gets the hash code of this migration error.
        /// </summary>
        public override int GetHashCode() => ErroneousVersion.GetHashCode();

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(MigrationError<TMigrationVersion> x, MigrationError<TMigrationVersion> y) => x.Equals(y);

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(MigrationError<TMigrationVersion> x, MigrationError<TMigrationVersion> y) => !x.Equals(y);
    }
}