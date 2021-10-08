using System;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.Linq2Db.Int64TimestampVersions
{
    /// <summary>
    /// Represents the default migration info for Int64 timestamps for Linq2Db.
    /// </summary>
    public class MigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Gets or sets the Id of the migration info (Primary Key).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Represents the default factory method that is used to instantiate a new <see cref="MigrationInfo" />.
        /// </summary>
        /// <param name="migration">The migration that was executed.</param>
        /// <param name="appliedAt">The point in time when the migration was applied. Use <see cref="DateTime.UtcNow" /> if possible.</param>
        public static MigrationInfo Create(Migration migration, DateTime appliedAt) =>
            new () { Version = migration.Version, Name = migration.Name, AppliedAt = appliedAt };
    }
}