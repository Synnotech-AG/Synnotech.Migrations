using System;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Captures an error that occurred during a migration.
    /// </summary>
    public sealed class MigrationError<TMigrationInfo>
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
        /// Throws a new <see cref="MigrationException" /> containing the actual <see cref="Exception" /> as an inner exception.
        /// </summary>
        /// <exception cref="MigrationException">Always thrown</exception>
        public void Rethrow() => throw new MigrationException($"Could not execute migration {MigrationWithError} successfully - please see the inner exception for details.", Exception);
    }
}