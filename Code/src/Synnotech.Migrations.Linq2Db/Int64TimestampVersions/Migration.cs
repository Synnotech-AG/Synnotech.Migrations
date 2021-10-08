using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.Linq2Db.Int64TimestampVersions
{
    /// <summary>
    /// Represents the default abstraction of a migration applied via Linq2Db.
    /// This type uses <see cref="long"/> values to identify and sort migrations.
    /// As the context type, <see cref="DataConnection"/> is used.
    /// </summary>
    public abstract class Migration : BaseMigration, IMigration<DataConnection>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Migration"/>.
        /// The version is retrieved via the <see cref="MigrationVersionAttribute"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the migration (optional). If the string is null, empty, or contains only white space,
        /// then the simple type name (not the fully-qualified name) is used.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown when the deriving class is not decorated with the <see cref="MigrationVersionAttribute" />.</exception>
        protected Migration(string? name = null) : base(name) { }

        /// <summary>
        /// Gets the value indicating whether this migration requires a transaction.
        /// The default value is true.
        /// Deriving classes can set this value.
        /// </summary>
        public bool IsRequiringTransaction { get; protected set; } = true;

        /// <summary>
        /// Executes the migration. Interactions with the target system can be performed
        /// using the specified data connection.
        /// IMPORTANT: you usually should not call 'dataConnection.CommitTransactionAsync' or something
        /// similar as this is handled by the migration engine.
        /// </summary>
        /// <param name="dataConnection">>The Linq2DB data connection that is used to interact with the target database.</param>
        /// <param name="cancellationToken">The token to cancel this asynchronous operation (optional).</param>
        public abstract Task ApplyAsync(DataConnection dataConnection, CancellationToken cancellationToken = default);
    }
}