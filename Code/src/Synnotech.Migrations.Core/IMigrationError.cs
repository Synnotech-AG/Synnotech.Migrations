using System;
using System.Diagnostics.CodeAnalysis;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of an error that occurred during a migration.
    /// </summary>
    public interface IMigrationError : IEquatable<IMigrationError?>
    {
        /// <summary>
        /// Gets the version of the erroneous migration as a string.
        /// </summary>
        string ErroneousVersionText { get; }

        /// <summary>
        /// Gets the exception that occurred during the migration.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Throws a new <see cref="MigrationException" /> containing the actual <see cref="Exception" /> as an inner exception.
        /// </summary>
        /// <exception cref="MigrationException">Always thrown</exception>
        [DoesNotReturn]
        void Rethrow();
    }
}