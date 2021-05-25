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

        /// <inheritdoc />
        public abstract Task ApplyAsync(IAsyncDocumentSession session);
    }
}