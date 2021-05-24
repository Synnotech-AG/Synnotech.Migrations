using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a migration that should be executed against the target system.
    /// This structure holds the migration version and the type that identifies
    /// the migration.
    /// </summary>
    /// <typeparam name="TMigrationVersion">The type that represents a migration version.</typeparam>
    public readonly struct PendingMigration<TMigrationVersion> : IEquatable<PendingMigration<TMigrationVersion>>, IComparable<PendingMigration<TMigrationVersion>>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PendingMigration{TMigrationVersion}" />.
        /// </summary>
        /// <param name="migrationVersion">The version of the migration.</param>
        /// <param name="migrationType">The type that represents the migration.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public PendingMigration(TMigrationVersion migrationVersion, Type migrationType)
        {
            MigrationVersion = migrationVersion.MustNotBeNullReference(nameof(migrationVersion));
            MigrationType = migrationType.MustNotBeNull(nameof(migrationType));
        }

        /// <summary>
        /// Gets the version of the migration.
        /// </summary>
        public TMigrationVersion MigrationVersion { get; }

        /// <summary>
        /// Gets the type that identifies the migration.
        /// </summary>
        public Type MigrationType { get; }

        /// <summary>
        /// Checks if the version and type are equal on this an the other instance.
        /// </summary>
        public bool Equals(PendingMigration<TMigrationVersion> other) =>
            MigrationVersion.Equals(other.MigrationVersion) && MigrationType == other.MigrationType;

        /// <summary>
        /// Compares this instance to the other instance, using the <see cref="MigrationVersion" /> to determine the order.
        /// </summary>
        public int CompareTo(PendingMigration<TMigrationVersion> other) => MigrationVersion.CompareTo(other.MigrationVersion);

        /// <summary>
        /// Checks if the specified object is a pending migration and has the same version and type as this instance.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is PendingMigration<TMigrationVersion> pendingMigration && Equals(pendingMigration);

        /// <summary>
        /// Gets the hash code of this pending migration.
        /// </summary>
        public override int GetHashCode() => MultiplyAddHash.CreateHashCode(MigrationVersion, MigrationType);

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(PendingMigration<TMigrationVersion> x, PendingMigration<TMigrationVersion> y) => x.Equals(y);

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(PendingMigration<TMigrationVersion> x, PendingMigration<TMigrationVersion> y) => !x.Equals(y);

        /// <summary>
        /// Converts a key value pair of version and type to an instance of <see cref="PendingMigration{TMigrationVersion}" />.
        /// </summary>
        public static PendingMigration<TMigrationVersion> FromKeyValuePair(KeyValuePair<TMigrationVersion, Type> keyValuePair) =>
            new (keyValuePair.Key, keyValuePair.Value);
    }
}