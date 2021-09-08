using System;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    /// <summary>
    /// Base class for migration infos that uses <see cref="long" /> timestamps.
    /// All properties are mutable to support serialization to document or relational databases
    /// with different frameworks.
    /// </summary>
    public abstract class BaseMigrationInfo : IHasMigrationVersion<long>
    {
        private string? _name;

        /// <summary>
        /// Gets or sets the version (as an Int64 timestamp).
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the name of the migration.
        /// </summary>
        public string? Name
        {
            get => _name;
            set => _name = value.MustNotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets or sets the date when the migration was applied. We recommend that you
        /// use UTC time stamps.
        /// </summary>
        public DateTime AppliedAt { get; set; }

        long IHasMigrationVersion<long>.GetMigrationVersion() => Version;

        /// <summary>
        /// Checks if the specified object is a migration info and ifg the version is the same.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is BaseMigrationInfo migrationInfo && Version == migrationInfo.Version;

        /// <summary>
        /// Returns the hash code for this migration info.
        /// </summary>
        // ReSharper disable once NonReadonlyMemberInGetHashCode -- Version cannot be readonly as instances of this class will be serialized. The serializer might need mutable properties for deserialization.
        public override int GetHashCode() => Version.GetHashCode();

        /// <summary>
        /// Returns the string representation fo this migration info.
        /// </summary>
        public override string ToString()
        {
            if (Version == 0L)
                return "Invalid Migration Info";
            return Version.ToString();
        }
    }
}