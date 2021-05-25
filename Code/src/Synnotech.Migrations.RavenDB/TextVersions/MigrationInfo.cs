using System;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default migration info for RavenDB.
    /// </summary>
    public class MigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Gets or sets the document ID of the migration info.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Represents the default factory method that is used to instantiate a new <see cref="MigrationInfo" />.
        /// </summary>
        /// <param name="migration">The migration that was executed.</param>
        /// <param name="appliedAt">The point in time when the migration was applied. Use <see cref="DateTime.UtcNow" /> if possible.</param>
        public static MigrationInfo Create(Migration migration, DateTime appliedAt)
        {
            var migrationInfo = new MigrationInfo { Name = migration.Name, AppliedAt = appliedAt };
            migrationInfo.SetInternalVersion(migration.Version);
            return migrationInfo;
        }
    }
}