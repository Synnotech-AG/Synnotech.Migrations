﻿using System;

namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    /// <summary>
    /// <para>
    /// Represents an asynchronous migration engine that uses <see cref="long" /> timestamp values
    /// to identify and order migrations. The info objects stored in the target system have
    /// to derive from <see cref="BaseMigrationInfo" />.
    /// </para>
    /// <para>
    /// The migration engine searches for migrations in
    /// the target assembly, compares them to the latest applied migration of the target system
    /// and then applies the pending migrations.
    /// </para>
    /// </summary>
    /// <typeparam name="TMigration">
    /// The base class that identifies all migrations. Must implement <see cref="IMigration{TContext}" /> and
    /// <see cref="IHasMigrationVersion{TMigrationVersion}" />
    /// </typeparam>
    /// <typeparam name="TMigrationInfo">
    /// The type whose instances are stored in the target system to indicate which migrations have been applied.
    /// </typeparam>
    /// <typeparam name="TMigrationContext">
    /// The type whose instances are passed to each migration when they are applied.
    /// </typeparam>
    public class MigrationEngine<TMigration, TMigrationInfo, TMigrationContext>
        : MigrationEngine<long, TMigration, MigrationVersionAttribute, TMigrationInfo, TMigrationContext>
        where TMigration : IMigration<TMigrationContext>, IHasMigrationVersion<long>
        where TMigrationInfo : BaseMigrationInfo
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationEngine{TMigration,TMigrationInfo,TMigrationContext}" />.
        /// </summary>
        /// <param name="sessionFactory">The factory that is used to create sessions which are used to access the target system.</param>
        /// <param name="migrationFactory">The factory that is used to instantiate migrations.</param>
        /// <param name="createMigrationInfo">The delegate that is used to instantiate migration infos.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public MigrationEngine(ISessionFactory<TMigrationInfo, TMigration, TMigrationContext> sessionFactory,
                               IMigrationFactory<TMigration> migrationFactory,
                               Func<TMigration, DateTime, TMigrationInfo> createMigrationInfo)
            : base(sessionFactory, migrationFactory, createMigrationInfo) { }
    }
}