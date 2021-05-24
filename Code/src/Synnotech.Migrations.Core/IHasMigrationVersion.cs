using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of an object that holds the migration version.
    /// </summary>
    /// <typeparam name="TMigrationVersion">The type that represents a migration version.</typeparam>
    public interface IHasMigrationVersion<out TMigrationVersion>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
    {
        /// <summary>
        /// Gets the migration version.
        /// </summary>
        /// <remarks>
        /// This member is a method deliberately (instead of a property), so that
        /// DTO types that hold the migration version can implement this interface
        /// without interfering with the default serialization rules.
        /// </remarks>
        TMigrationVersion GetMigrationVersion();
    }
}