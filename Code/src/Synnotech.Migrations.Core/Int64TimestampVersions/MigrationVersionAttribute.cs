using System;

namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    /// <summary>
    /// Represents the version of a migration that can be obtained via reflection.
    /// The version is a <see cref="long" /> value that represents a timestamp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MigrationVersionAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<long>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationVersionAttribute" />.
        /// </summary>
        /// <param name="version">The <see cref="long" /> timestamp that uniquely identifies the migration.</param>
        public MigrationVersionAttribute(long version) => Version = version;

        /// <summary>
        /// Gets the version of the migration.
        /// </summary>
        public long Version { get; }

        long IHasMigrationVersion<long>.GetMigrationVersion() => Version;

        /// <summary>
        /// Does nothing - the version cannot be parsed in a wrong manner currently.
        /// </summary>
        public void Validate(Type migrationType) { }
    }
}