using System;
using System.Data.Entity;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Represents the default migration engine for EntityFramework.
/// </summary>
public class MigrationEngine<TDbContext> : MigrationEngine<Migration<TDbContext>, MigrationInfo, TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="MigrationEngine{TDbContext}" />.
    /// </summary>
    /// <param name="sessionFactory">The factory that is used to create sessions which are used to access the target system.</param>
    /// <param name="migrationFactory">The factory that is used to instantiate migrations.</param>
    /// <param name="createMigrationInfo">The delegate that is used to instantiate migration infos.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    public MigrationEngine(ISessionFactory<MigrationInfo, Migration<TDbContext>, TDbContext> sessionFactory,
                           IMigrationFactory<Migration<TDbContext>> migrationFactory,
                           Func<Migration<TDbContext>, DateTime, MigrationInfo> createMigrationInfo)
        : base(sessionFactory, migrationFactory, createMigrationInfo) { }
}