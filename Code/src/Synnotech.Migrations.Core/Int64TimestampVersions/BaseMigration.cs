namespace Synnotech.Migrations.Core.Int64TimestampVersions
{
    /// <summary>
    /// Base class for migrations that use the <see cref="MigrationVersionAttribute" /> to indicate
    /// the migrations version as a <see cref="long" /> timestamp.
    /// </summary>
    public abstract class BaseMigration : BaseMigration<long, MigrationVersionAttribute>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BaseMigration" />.
        /// </summary>
        /// <param name="name">
        /// The name of the migration (optional). If the string is null, empty, or contains only white space,
        /// then the simple type name (not the fully-qualified name) is used.
        /// </param>
        protected BaseMigration(string? name = null) : base(name) { }

        /// <inheritdoc />
        public override string ConvertVersionToString() => Version.ToString();
    }
}