using System;
using Light.GuardClauses;
#if NETSTANDARD2_1
using Range = Light.GuardClauses.Range;
#endif

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Base class for migrations that use the <see cref="MigrationVersionAttribute" /> to indicate
    /// the migration version as a string that can be parsed as a <see cref="Version" /> instance.
    /// </summary>
    public abstract class BaseMigration : BaseMigration<Version, MigrationVersionAttribute>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BaseMigration" />.
        /// </summary>
        /// <param name="name">
        /// The name of the migration (optional). If the string is null, empty, or contains only white space,
        /// then the simple type name (not the fully-qualified name) is used.
        /// </param>
        /// <param name="fieldCount">The number of components included when the version of this migration is turned into a string. The default is 3 (semantic versions).</param>
        protected BaseMigration(string? name = null, int fieldCount = 3) : base(name) =>
            FieldCount = fieldCount.MustBeIn(Range.FromInclusive(1).ToInclusive(4), nameof(fieldCount));

        /// <summary>
        /// Gets the number of fields that will be returned when the version is converted to a string.
        /// </summary>
        protected int FieldCount { get; }

        /// <inheritdoc />
        public override string ConvertVersionToString() => Version.ToString(FieldCount);
    }
}