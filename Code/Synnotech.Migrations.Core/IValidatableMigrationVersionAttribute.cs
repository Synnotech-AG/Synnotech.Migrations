using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of a migration version attribute.
    /// </summary>
    public interface IMigrationVersionAttribute
    {
        /// <summary>
        /// Validates that the version value passed to the attribute is valid.
        /// This method should throw an exception if the specified value is invalid.
        /// The migration type can be used to indicate which migration is invalid.
        /// </summary>
        /// <param name="migrationType">The migration type that applied the migration version attribute.</param>
        void Validate(Type migrationType);
    }
}