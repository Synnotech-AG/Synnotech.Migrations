using System;
using LinqToDB.Data;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default migration engine for Linq2Db.
    /// </summary>
    public class MigrationEngine : MigrationEngine<Migration, MigrationInfo, DataConnection>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationEngine" />.
        /// </summary>
        /// <param name="sessionFactory">The factory that is used to create sessions which are used to access the target system.</param>
        /// <param name="migrationFactory">The factory that is used to instantiate migrations.</param>
        /// <param name="createMigrationInfo">The delegate that is used to instantiate migration infos.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public MigrationEngine(ISessionFactory<MigrationInfo, Migration, DataConnection> sessionFactory,
                               IMigrationFactory<Migration> migrationFactory,
                               Func<Migration, DateTime, MigrationInfo> createMigrationInfo)
            : base(sessionFactory, migrationFactory, createMigrationInfo) { }
    }
}