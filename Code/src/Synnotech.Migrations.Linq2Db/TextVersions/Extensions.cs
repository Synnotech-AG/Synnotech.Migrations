using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using LinqToDB.Mapping;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Provides extension methods for LinqToDb.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers the mappings for the <see cref="MigrationInfo" /> instance.
        /// </summary>
        /// <param name="builder">The LinqToDB builder that is used to create mappings from model classes to database tables.</param>
        /// <param name="tableName">The name of the table that will hold records for migration infos (optional). Defaults to "MigrationInfos".</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder" /> or <paramref name="tableName" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="tableName" /> is an empty string.</exception>
        /// <exception cref="WhiteSpaceStringException">Thrown when <paramref name="tableName" /> contains only white space.</exception>
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