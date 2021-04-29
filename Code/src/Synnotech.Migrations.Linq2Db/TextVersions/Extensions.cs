using Light.GuardClauses;
using LinqToDB.Mapping;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Provides extension methods for LinqToDb.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers the mappings for the <see cref="MigrationInfo"/> instance.
        /// </summary>
        /// <param name="builder">The LinqToDB builder that is used to create mappings from model classes to database tables.</param>
        /// <param name="tableName">The name of the table that will hold records for migration infos (optional). Defaults</param>
        /// <returns></returns>
        public static FluentMappingBuilder MapMigrationInfo(this FluentMappingBuilder builder, string tableName = "MigrationInfos")
        {
            builder.MustNotBeNull(nameof(builder));
            tableName.MustNotBeNullOrWhiteSpace(nameof(tableName));

            builder.Entity<MigrationInfo>()
                   .HasTableName(tableName)
                   .Property(info => info.Id).IsIdentity().IsPrimaryKey()
                   .Property(info => info.Name).HasLength(100).IsNullable(false)
                   .Property(info => info.Version).HasLength(20).IsNullable(false);
            return builder;
        }
    }
}