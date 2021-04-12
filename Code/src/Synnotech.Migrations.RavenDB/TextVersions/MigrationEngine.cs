using System;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default async migration engine for RavenDB.
    /// </summary>
    public class MigrationEngine : MigrationEngine<MigrationSession, Migration, MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationEngine" />.
        /// </summary>
        /// <param name="sessionFactory">The factory that creates new session instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sessionFactory"/> is null.</exception>
        public MigrationEngine(ISessionFactory<MigrationSession, Migration> sessionFactory)
            : base(sessionFactory, new AttributeMigrationsProvider<Migration, MigrationInfo>(), MigrationInfo.Create) { }
    }
}