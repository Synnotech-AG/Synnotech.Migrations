using System;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default async migration engine for RavenDB that uses <see cref="Version" /> instances
    /// to represent migration versions.
    /// </summary>
    public class MigrationEngine : MigrationEngine<Migration, MigrationInfo, IAsyncDocumentSession>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationEngine" />.
        /// </summary>
        /// <param name="sessionFactory">The factory that is used to create sessions which are used to access the target system.</param>
        /// <param name="migrationFactory">The factory that is used to instantiate migrations.</param>
        /// <param name="createMigrationInfo">The delegate that is used to instantiate migration infos.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public MigrationEngine(ISessionFactory<MigrationInfo, Migration, IAsyncDocumentSession> sessionFactory,
                               IMigrationFactory<Migration> migrationFactory,
                               Func<Migration, DateTime, MigrationInfo> createMigrationInfo)
            : base(sessionFactory, migrationFactory, createMigrationInfo) { }
    }
}