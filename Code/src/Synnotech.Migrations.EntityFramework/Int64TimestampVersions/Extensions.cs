using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using System;
using System.Data.Entity;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Provides extension methods for EntityFramework.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the mappings for the <see cref="MigrationInfo" /> instance.
    /// </summary>
    /// <param name="modelBuilder">The EntityFramework model builder that is used to define the database model.</param>
    /// <param name="tableName">The name of the table that will hold records for migration infos (optional). Defaults to "MigrationInfos".</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="modelBuilder" /> or <paramref name="tableName" /> is null.</exception>
    /// <exception cref="EmptyStringException">Thrown when <paramref name="tableName" /> is an empty string.</exception>
    /// <exception cref="WhiteSpaceStringException">Thrown when <paramref name="tableName" /> contains only white space.</exception>
    public static  DbModelBuilder ConfigureMigrationInfo(this DbModelBuilder modelBuilder, string tableName = "MigrationInfos")
    {
        modelBuilder.MustNotBeNull(nameof(modelBuilder));
        tableName.MustNotBeNullOrWhiteSpace(nameof(tableName));

        var migrationInfoEntity = modelBuilder.Entity<MigrationInfo>();

        migrationInfoEntity.ToTable(tableName)
                           .HasKey(info => info.Id);

        migrationInfoEntity.Property(info => info.Name)
                           .HasMaxLength(100)
                           .IsRequired();

        migrationInfoEntity.Property(info => info.Version)
                           .IsRequired();

        return modelBuilder;
    }
}