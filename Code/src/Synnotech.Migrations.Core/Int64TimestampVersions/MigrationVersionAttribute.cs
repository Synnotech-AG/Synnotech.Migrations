using System;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    /// <summary>
    /// Represents the version of a migration that can be obtained via reflection.
    /// The version is a <see cref="long" /> value that should represent a timestamp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MigrationVersionAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<long>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationVersionAttribute" />. With this constructor,
        /// you can pass in the Int64 value directly. We recommend you use the other constructor that
        /// parses an ISO 8601 UTC timestamp string to avoid malformed version values.
        /// </summary>
        /// <param name="version">The <see cref="long" /> timestamp that uniquely identifies the migration.</param>
        public MigrationVersionAttribute(long version) => Version = version;

        /// <summary>
        /// Initializes a new instance of <see cref="MigrationVersionAttribute"/>. You should prefer this
        /// constructor as the specified string is parsed as an ISO 8601 UTC timestamp, which avoids malformed
        /// versions.
        /// </summary>
        /// <param name="iso8601UtcTimestamp">
        /// A string that represents
        /// </param>
        public MigrationVersionAttribute(string iso8601UtcTimestamp)
        {
            Iso8601UtcTimestamp = iso8601UtcTimestamp;
            if (TimestampParser.TryParseTimestamp(iso8601UtcTimestamp, out var version))
                Version = version;
        }

        /// <summary>
        /// Gets the version of the migration.
        /// </summary>
        public long Version { get; }

        private string? Iso8601UtcTimestamp { get; }

        long IHasMigrationVersion<long>.GetMigrationVersion() => Version;

        /// <summary>
        /// Checks if a possible ISO 8601 UTC timestamp is valid, or throws a <see cref="MigrationException" /> otherwise.
        /// </summary>
        public void Validate(Type migrationType)
        {
            if (Iso8601UtcTimestamp != null && Version == 0L)
                throw new MigrationException($"The specified version \"{Iso8601UtcTimestamp}\" of migration {migrationType.ToStringOrNull()} cannot be parsed");
        }
    }
}