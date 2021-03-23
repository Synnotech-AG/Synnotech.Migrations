using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// This migrations provider uses attributes to identify which migrations
    /// need to be applied to the target system.
    /// </summary>
    /// <typeparam name="TMigration">The type that represents the general abstraction for migrations.</typeparam>
    /// <typeparam name="TMigrationInfo">The type that is stored in the target system to identify which migrations have already been applied.</typeparam>
    /// <typeparam name="TMigrationAttribute">The attribute that identifies the version of a migration.</typeparam>
    public class AttributeMigrationsProvider<TMigration, TMigrationInfo, TMigrationAttribute> : IMigrationsProvider<TMigration, TMigrationInfo>
        where TMigrationInfo : IComparable<TMigrationAttribute>
        where TMigrationAttribute : Attribute, IMigrationVersionAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AttributeMigrationsProvider{TMigration,TMigrationInfo,TMigrationAttribute}" />.
        /// </summary>
        /// <param name="instantiateMigration">The optional delegate that is used to instantiate a migration. If not provided, then <see cref="InstantiateMigrationUsingActivator"/> is used.</param>
        public AttributeMigrationsProvider(Func<Type, TMigration>? instantiateMigration = null) =>
            InstantiateMigration = instantiateMigration ?? InstantiateMigrationUsingActivator;

        /// <summary>
        /// Gets the delegate that is used to instantiate a migration from a type.
        /// </summary>
        public Func<Type, TMigration> InstantiateMigration { get; }

        /// <inheritdoc />
        public List<TMigration>? DetermineMigrations(Assembly migrationAssembly, TMigrationInfo? latestMigrationInfo)
        {
            migrationAssembly.MustNotBeNull(nameof(migrationAssembly));
            var migrationAbstractionType = typeof(TMigration);

            var hashSet = new HashSet<TMigration>();
            foreach (var type in migrationAssembly.ExportedTypes)
            {
                if (!type.IsClass || type.IsAbstract || !type.DerivesFrom(migrationAbstractionType))
                    continue;

                var migrationAttribute = type.GetCustomAttribute<TMigrationAttribute>();
                if (migrationAttribute == null)
                    continue;

                migrationAttribute.Validate(type);

                if (latestMigrationInfo != null && latestMigrationInfo.CompareTo(migrationAttribute) >= 0)
                    continue;

                var migrationInstance = InstantiateMigration(type);
                if (!hashSet.Add(migrationInstance))
                    throw new MigrationException($"The migration {migrationInstance} is a duplicate.");
            }

            return hashSet.Count switch
            {
                0 => null,
                1 => new List<TMigration>(hashSet),
                _ => hashSet.OrderBy(migration => migration).ToList()
            };
        }

        /// <summary>
        /// This method instantiates a type using <see cref="Activator.CreateInstance(Type)" />.
        /// This requires the migration type to have a default constructor.
        /// </summary>
        /// <param name="type">The type to be instantiated.</param>
        /// <exception cref="MigrationException">Thrown when the type could not be instantiated or the instance cannot be cast to <typeparamref name="TMigration"/>.</exception>
        public static TMigration InstantiateMigrationUsingActivator(Type type)
        {
            object? instance;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not instantiate migration type \"{type}\".", exception);
            }

            if (instance is TMigration migration)
                return migration;

            throw new MigrationException($"Type \"{type}\" cannot be cast to migration type \"{typeof(TMigration)}\".");
        }
    }
}