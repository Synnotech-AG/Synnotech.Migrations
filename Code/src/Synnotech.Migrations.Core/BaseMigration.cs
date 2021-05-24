using System;
using System.Reflection;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Base class for migrations. Implements the <see cref="IHasMigrationVersion{TMigrationVersion}" /> interface
    /// and provides several default members like <see cref="Name" />.
    /// </summary>
    /// <typeparam name="TMigrationVersion"></typeparam>
    /// <typeparam name="TMigrationAttribute"></typeparam>
    public abstract class BaseMigration<TMigrationVersion, TMigrationAttribute> : IHasMigrationVersion<TMigrationVersion>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        where TMigrationAttribute : Attribute, IHasMigrationVersion<TMigrationVersion>
    {
        protected BaseMigration(string? name = null)
        {
            var type = GetType();
            var migrationAttribute = type.GetCustomAttribute<TMigrationAttribute>();
            if (migrationAttribute == null)
                throw new InvalidOperationException($"The {typeof(TMigrationAttribute).Name} is not applied to migration \"{type}\".");
            Version = migrationAttribute.GetMigrationVersion();
            Name = name ?? type.Name;
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
        /// Returns the string representation of the migration (version & name).
        /// </summary>
        public override string ToString() => ConvertVersionToString() + " " + Name;
    }
}