using System;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Represents the version of a migration that can be obtained via reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MigrationVersionAttribute : Attribute, IMigrationVersionAttribute
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


        public void Validate(Type migrationType)
        {
            if (_version == null)
                throw new ArgumentException($"The specified version \"{_versionText}\" of migration \"{migrationType}\" cannot be parsed.");
        }
    }
}