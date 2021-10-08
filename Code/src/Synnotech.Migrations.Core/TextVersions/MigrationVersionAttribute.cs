using System;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Represents the version of a migration that can be obtained via reflection.
    /// The version is a <see cref="string" /> that will be parsed to a <see cref="Version" />
    /// instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MigrationVersionAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<Version>
    {
        private readonly Version? _version;
        private readonly string _versionText;

        /// <summary>
        /// Initializes a new instance <see cref="MigrationVersionAttribute" />.
        /// </summary>
        /// <param name="version">
        /// The string that describes the version. This string will be parsed to a <see cref="System.Version" /> instance.
        /// We recommend that you use semantic versions.
        /// </param>
        public MigrationVersionAttribute(string version)
        {
            _versionText = version;
            if (Version.TryParse(version, out var parsedVersion))
                _version = parsedVersion;
        }

        /// <summary>
        /// Gets the version of the migration.
        /// </summary>
        public Version Version => _version ?? throw new ArgumentException($"The specified version {_versionText.ToStringOrNull()} cannot be parsed.");

        Version IHasMigrationVersion<Version>.GetMigrationVersion() => Version;

        /// <summary>
        /// Throws a migration exception when <see cref="Version" /> was not set properly.
        /// </summary>
        public void Validate(Type migrationType)
        {
            if (_version == null)
                throw new MigrationException($"The specified version {_versionText.ToStringOrNull()} of migration {migrationType.ToStringOrNull()} cannot be parsed.");
        }
    }
}