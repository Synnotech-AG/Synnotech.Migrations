using System;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// This migrations provider uses the <see cref="MigrationVersionAttribute" /> to identify which migrations
    /// need to be applied to the target system.
    /// </summary>
    /// <typeparam name="TMigration">The type that represents the general abstraction for migrations.</typeparam>
    /// <typeparam name="TMigrationInfo">The type that is stored in the target system to identify which migrations have already been applied.</typeparam>
    public sealed class AttributeMigrationsProvider<TMigration, TMigrationInfo> : AttributeMigrationsProvider<TMigration, TMigrationInfo, MigrationVersionAttribute>
        where TMigrationInfo : class, IComparable<MigrationVersionAttribute>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AttributeMigrationsProvider{TMigration,TMigrationInfo}" />.
        /// </summary>
        /// <param name="instantiateMigration">The optional delegate that is used to instantiate a migration. If not provided, then <see cref="AttributeMigrationsProvider{TMigration,TMigrationInfo,TMigrationAttribute}.InstantiateMigrationUsingActivator" /> is used.</param>
        public AttributeMigrationsProvider(Func<Type, TMigration>? instantiateMigration = null) : base(instantiateMigration) { }
    }
}