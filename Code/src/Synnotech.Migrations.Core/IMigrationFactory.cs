using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the abstraction of a factory that instantiates migrations.
    /// </summary>
    /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
    public interface IMigrationFactory<out TMigration>
    {
        /// <summary>
        /// Creates a new migration instance.
        /// </summary>
        /// <param name="type">The type that represents the migration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        TMigration CreateMigration(Type type);
    }
}