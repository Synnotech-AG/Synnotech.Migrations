using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Represents the default abstraction of a migration applied to RavenDB. This type
    /// uses <see cref="System.Version" /> to identify and sort migrations. As the context type,
    /// <see cref="IAsyncDocumentSession" /> is used.
    /// </summary>
    public abstract class Migration : BaseMigration, IMigration<IAsyncDocumentSession>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Migration" />.
        /// </summary>
        /// <param name="name">
        /// The optional name of the migration. If null, <see cref="string.Empty" />, or a string containing only white-space
        /// is specified, the class name is used as name instead.
        /// </param>
        protected Migration(string? name = null) : base(name) { }

        /// <summary>
        /// Executes the migration. Interactions with the target system can be performed
        /// using the <paramref name="session"/>.
        /// IMPORTANT: you usually should not call 'session.SaveChangesAsync' or something
        /// similar as this is handled by the migration engine.
        /// Furthermore, WaitForIndexesOnSaveChanges is enabled by default, so you can safely query
        /// changes that were made by previous migrations that were just executed.
        /// </summary>
        /// <param name="session">The RavenDB session used to interact with the database.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public abstract Task ApplyAsync(IAsyncDocumentSession session, CancellationToken cancellationToken = default);
    }
}