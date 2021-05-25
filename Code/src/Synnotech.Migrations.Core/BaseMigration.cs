using System;
using System.Reflection;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Base class for migrations. Implements the <see cref="IHasMigrationVersion{TMigrationVersion}" /> interface
    /// and provides several default members like <see cref="Name" />.
    /// </summary>
    /// <typeparam name="TMigrationVersion">The type that represents a migration version. It must be equatable and comparable.</typeparam>
    /// <typeparam name="TMigrationAttribute">
    /// The type that represents the attribute being applied to migrations to indicate their version.
    /// Must implement <see cref="IHasMigrationVersion{TMigrationVersion}"/>.
    /// </typeparam>
    public abstract class BaseMigration<TMigrationVersion, TMigrationAttribute> : IHasMigrationVersion<TMigrationVersion>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        where TMigrationAttribute : Attribute, IHasMigrationVersion<TMigrationVersion>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BaseMigration{TMigrationVersion,TMigrationAttribute}"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the migration (optional). If the string is null, empty, or contains only white space,
        /// then the simple type name (not the fully-qualified name) is used.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown when the migration attribute is not applied properly to the subclass.</exception>
        protected BaseMigration(string? name = null)
        {
            var type = GetType();
            var migrationAttribute = type.GetCustomAttribute<TMigrationAttribute>();
            if (migrationAttribute == null)
                throw new InvalidOperationException($"The {typeof(TMigrationAttribute).Name} is not applied to migration \"{type}\".");
            Version = migrationAttribute.GetMigrationVersion();
            Name = name.IsNullOrWhiteSpace() ? type.Name : name;
        }

        /// <summary>
        /// Gets the version of this migration.
        /// </summary>
        public TMigrationVersion Version { get; }

        /// <summary>
        /// Gets the name of this migration.
        /// </summary>
        public string Name { get; }

        TMigrationVersion IHasMigrationVersion<TMigrationVersion>.GetMigrationVersion() => Version;

        /// <summary>
        /// Converts the version to a string.
        /// </summary>
        public abstract string ConvertVersionToString();

        /// <summary>
        /// Checks if the specified object is a migration and has the same version.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is BaseMigration<TMigrationVersion, TMigrationAttribute> migration && Version.Equals(migration.Version);

        /// <summary>
        /// Returns the hash code of the version.
        /// </summary>
        public override int GetHashCode() => Version.GetHashCode();

        /// <summary>
        /// Returns the string representation of the migration (version and name).
        /// </summary>
        public override string ToString() => ConvertVersionToString() + " " + Name;
    }
}