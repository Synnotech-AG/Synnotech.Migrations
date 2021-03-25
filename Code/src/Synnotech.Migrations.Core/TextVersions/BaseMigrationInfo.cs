using System;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;
using Range = Light.GuardClauses.Range;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Base class for default migration infos that uses a version internally.
    /// This version is serialized as a string. by default in the format x.x.x
    /// (semantic version). All properties are mutable to support
    /// serialization to document or relational databases with different frameworks.
    /// </summary>
    public abstract class BaseMigrationInfo : IComparable<MigrationVersionAttribute>
    {
        private readonly int _fieldCount;
        private string? _name;
        private Version? _version;

        /// <summary>
        /// Initializes a new instance of <see cref="BaseMigrationInfo"/>.
        /// </summary>
        /// <param name="fieldCount">The number of components that are returned from the internal version when it is serialized to a string. Default is 3 (semantic version).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fieldCount"/> is not in between 1 to 4</exception>
        protected BaseMigrationInfo(int fieldCount = 3)
        {
            _fieldCount = fieldCount.MustBeIn(Range.FromInclusive(1).ToInclusive(4), nameof(fieldCount));
        }

        /// <summary>
        /// Gets or sets the version as a string.
        /// </summary>
        public string? Version
        {
            get => _version?.ToString(_fieldCount);
            set => _version = new Version(value.MustNotBeNullOrWhiteSpace());
        }

        /// <summary>
        /// Gets or sets the name of the migration.
        /// </summary>
        public string? Name
        {
            get => _name;
            set => _name = value.MustNotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Compares this migration info to the version attribute of a migration.
        /// </summary>
        /// <param name="other">The attribute that holds the migration of a migration.</param>
        /// <returns>
        /// A negative value when the internal version is null or less than the version of the attribute.
        /// If both versions are equal, null is returned. Otherwise, a positive number is returned.
        /// </returns>
        public int CompareTo(MigrationVersionAttribute? other)
        {
            other = other.MustNotBeNull(nameof(other));

            if (_version == null)
                return -1;

            return _version.CompareTo(other.Version);
        }

        /// <summary>
        /// Gets the internal version if it is set.
        /// </summary>
        /// <param name="version">The internal version.</param>
        /// <returns>True if the internal version is set, else false.</returns>
        public bool TryGetInternalVersion([NotNullWhen(true)] out Version? version)
        {
            if (_version == null)
            {
                version = default;
                return false;
            }

            version = _version;
            return true;
        }

        /// <summary>
        /// Sets the internal version to the specified value.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="version"/> is null.</exception>
        public void SetInternalVersion(Version version)
        {
            _version = version.MustNotBeNull(nameof(version));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (_version == null)
                return "Invalid Migration Info";

            var version = Version!;
            return _name == null ? version : version + " " + _name;
        }

        /// <summary>
        /// Checks if the specified object is a migration info and if the version is the same.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is BaseMigrationInfo migrationInfo && _version == migrationInfo._version;

        /// <summary>
        /// Gets the hash code for this migration info.
        /// </summary>
        // ReSharper disable once NonReadonlyMemberInGetHashCode -- _version cannot be readonly as this DTO might be used by ORMs / serializers that do not support constructor injection
        public override int GetHashCode() => _version?.GetHashCode() ?? 0;
    }
}