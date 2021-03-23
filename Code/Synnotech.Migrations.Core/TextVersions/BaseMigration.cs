using System;
using System.Reflection;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Base class for default migrations. It retrieves the version
    /// via the <see cref="MigrationVersionAttribute" />.
    /// </summary>
    /// <typeparam name="TMigration">The type of the derived migration class.</typeparam>
    public abstract class BaseMigration<TMigration> : IEquatable<TMigration>, IComparable<TMigration>
        where TMigration : BaseMigration<TMigration>
    {
        private readonly int _fieldCount;

        /// <summary>
        /// Initializes a new instance of <see cref="BaseMigration{TMigration}" />.
        /// The version is retrieved via the <see cref="MigrationVersionAttribute" />.
        /// </summary>
        /// <param name="name">The name of the migration (optional). If null is specified, then the name will be retrieved from <see cref="object.GetType"/>.</param>
        /// <param name="fieldCount">The number of components included when the version of this migration is turned into a string. The default is 3 (semantic versions).</param>
        /// <exception cref="InvalidOperationException">Thrown when the deriving class is not decorated with the <see cref="MigrationVersionAttribute"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fieldCount"/> is not in between 1 and 4.</exception>
        protected BaseMigration(string? name = null, int fieldCount = 3)
        {
            _fieldCount = fieldCount.MustBeIn(Range.FromInclusive(1).ToInclusive(4), nameof(fieldCount));
            var type = GetType();
            var migrationVersionAttribute = type.GetCustomAttribute<MigrationVersionAttribute>();
            if (migrationVersionAttribute == null)
                throw new InvalidOperationException($"The {nameof(MigrationVersionAttribute)} is not applied to migration \"{type}\".");
            Version = migrationVersionAttribute.Version;
            Name = name ?? type.Name;
        }

        /// <summary>
        /// Gets the version of this migration.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the version as a string.
        /// </summary>
        public string VersionString => Version.ToString(_fieldCount);

        /// <summary>
        /// Gets the name of this migration.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Checks if the other migration is equal to this one.
        /// </summary>
        /// <param name="other">The other migration instance.</param>
        /// <returns>True if other points to the same instance as this, or if the other version is equal to this instance's version.</returns>
        public bool Equals(TMigration? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;

            return Version == other.Version;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is TMigration migration && Equals(migration);

        /// <inheritdoc />
        public override int GetHashCode() => Version.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => VersionString + " " + Name;

        /// <summary>
        /// Compares the versions of this instance and the specified other instance.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
        public int CompareTo(TMigration? other) =>
            Version.CompareTo(other.MustNotBeNull(nameof(other)).Version);
    }
}