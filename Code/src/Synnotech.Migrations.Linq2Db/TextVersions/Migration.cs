using System;
using System.Threading.Tasks;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents the default abstraction of a migration applied via Linq2Db. This type
    /// uses <see cref="System.Version" /> to identify and sort migrations. As the context type,
    /// <see cref="MigrationSession"/> is used.
    /// </summary>
    public abstract class Migration : BaseMigration<Migration>, IMigration<MigrationSession>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Migration" />.
        /// The version is retrieved via the <see cref="MigrationVersionAttribute" />.
        /// </summary>
        /// <param name="name">The name of the migration (optional). If null is specified, then the name will be retrieved from <see cref="object.GetType"/>.</param>
        /// <param name="fieldCount">The number of components included when the version of this migration is turned into a string. The default is 3 (semantic versions).</param>
        /// <exception cref="InvalidOperationException">Thrown when the deriving class is not decorated with the <see cref="MigrationVersionAttribute"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fieldCount"/> is not in between 1 and 4.</exception>
        protected Migration(string? name = null, int fieldCount = 3) : base(name, fieldCount) { }

        /// <summary>
        /// Gets the value indicating whether this migration requires a transaction.
        /// The default value is true.
        /// Deriving classes can set this value.
        /// </summary>
        public bool IsRequiringTransaction { get; protected set; } = true;

        /// <summary>
        /// Executes the migration. Interactions with the target system can be performed
        /// using the specified session.
        /// IMPORTANT: you usually should not call 'context.SaveChangesAsync' or something
        /// similar as this is handled by the migration engine.
        /// You can set <see cref="IsRequiringTransaction"/> to false to handle transactions yourself
        /// or execute the migration without a transaction.
        /// </summary>
        /// <param name="session">The object that is used to interact with the target database.</param>
        public abstract Task ApplyAsync(MigrationSession session);
    }
}