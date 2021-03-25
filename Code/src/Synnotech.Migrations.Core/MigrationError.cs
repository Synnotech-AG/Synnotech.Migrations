using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Captures an error that occurred during a migration.
    /// </summary>
    public sealed class MigrationError<TMigrationInfo> : IEquatable<MigrationError<TMigrationInfo>>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationError{TMigrationInfo}" />.
        /// </summary>
        /// <param name="migrationWithError">Metadata about the migration that could not be executed.</param>
        /// <param name="exception">The exception that occurred during the migration.</param>
        public MigrationError(TMigrationInfo migrationWithError, Exception exception)
        {
            MigrationWithError = migrationWithError.MustNotBeNullReference(nameof(migrationWithError));
            Exception = exception.MustNotBeNull(nameof(exception));
        }

        /// <summary>
        /// Gets metadata about the migration that could not be executed.
        /// </summary>
        public TMigrationInfo MigrationWithError { get; }

        /// <summary>
        /// Gets the exception that was thrown when the migration was executed.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Checks if the specified migration error is equal to this instance.
        /// This is true when the migration info represents the same migration as the other instance.
        /// </summary>
        public bool Equals(MigrationError<TMigrationInfo>? other)
        {
            if (other is null)
                return false;
            return EqualityComparer<TMigrationInfo>.Default.Equals(MigrationWithError, other.MigrationWithError);
        }

        /// <summary>
        /// Throws a new <see cref="MigrationException" /> containing the actual <see cref="Exception" /> as an inner exception.
        /// </summary>
        /// <exception cref="MigrationException">Always thrown</exception>
        public void Rethrow() => throw new MigrationException($"Could not execute migration {MigrationWithError} successfully - please see the inner exception for details.", Exception);

        /// <summary>
        /// Checks if the specified object is a migration error and equal to this instance.
        /// This is true when the migration info represents the same migration as the other instance.
        /// </summary>
        public override bool Equals(object obj) => obj is MigrationError<TMigrationInfo> error && Equals(error);

        /// <summary>
        /// Gets the hash code of this migration error.
        /// </summary>
        public override int GetHashCode() => MigrationWithError!.GetHashCode();

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(MigrationError<TMigrationInfo>? x, MigrationError<TMigrationInfo>? y)
        {
            if (x is null)
                return y is null;
            return x.Equals(y);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(MigrationError<TMigrationInfo>? x, MigrationError<TMigrationInfo>? y) => !(x == y);
    }
}