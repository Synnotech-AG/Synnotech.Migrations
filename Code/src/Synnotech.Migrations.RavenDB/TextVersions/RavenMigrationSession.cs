using System;
using Raven.Client.Documents.Session;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the session that is used to apply a migration and store the corresponding migration info.
    /// <see cref="MigrationInfo" /> is used as the type that represents a migration info.
    /// </summary>
    public sealed class RavenMigrationSession : RavenMigrationSession<MigrationInfo>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RavenMigrationSession" />.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session" /> is null.</exception>
        public RavenMigrationSession(IAsyncDocumentSession session) : base(session) { }
    }
}