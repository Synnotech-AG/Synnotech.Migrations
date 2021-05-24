using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Provides methods to determine the pending migrations of assemblies.
    /// </summary>
    public static class PendingMigrations
    {
        /// <summary>
        /// Searches the specified assemblies for types that represent migrations and have a higher migration version than the specified one.
        /// </summary>
        /// <typeparam name="TMigrationVersion">The type that represents a migration version.</typeparam>
        /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
        /// <typeparam name="TMigrationAttribute">The type that represents the attribute being applied to migrations to indicate their version.</typeparam>
        /// <param name="latestVersion">The latest version that is already applied to the target system.</param>
        /// <param name="assembliesContainingMigrations">The assemblies that will be searched for migration types.</param>
        /// <returns>A list of pending migrations that should be applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembliesContainingMigrations"/> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assembliesContainingMigrations"/> is an empty array.</exception>
        public static List<PendingMigration<TMigrationVersion>>? DetermineNewMigrations<TMigrationVersion, TMigration, TMigrationAttribute>(TMigrationVersion? latestVersion,
                                                                                                                                            Assembly[] assembliesContainingMigrations)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        {
            assembliesContainingMigrations.MustNotBeNullOrEmpty(nameof(assembliesContainingMigrations));

            var migrationBaseType = typeof(TMigration);
            var foundMigrationTypes = new Dictionary<TMigrationVersion, Type>();
            foreach (var assembly in assembliesContainingMigrations)
            {
                FindNewerMigrationTypes<TMigrationVersion, TMigrationAttribute>(assembly, latestVersion, migrationBaseType, foundMigrationTypes);
            }

            return foundMigrationTypes.Count switch
            {
                0 => null,
                1 => new List<PendingMigration<TMigrationVersion>>(1) { PendingMigration<TMigrationVersion>.FromKeyValuePair(foundMigrationTypes.First()) },
                _ => foundMigrationTypes.OrderBy(keyValuePair => keyValuePair.Key)
                                        .Select(PendingMigration<TMigrationVersion>.FromKeyValuePair)
                                        .ToList()
            };
        }

        private static void FindNewerMigrationTypes<TMigrationId, TMigrationAttribute>(Assembly assembly,
                                                                                       TMigrationId? latestVersion,
                                                                                       Type migrationBaseType,
                                                                                       Dictionary<TMigrationId, Type> foundMigrationTypes)
            where TMigrationId : IEquatable<TMigrationId>, IComparable<TMigrationId>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationId>
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (!type.IsClass || type.IsAbstract || !type.DerivesFrom(migrationBaseType))
                    continue;

                var migrationAttribute = type.GetCustomAttribute<TMigrationAttribute>();
                if (migrationAttribute == null)
                    continue;

                migrationAttribute.Validate(type);

                var migrationVersion = migrationAttribute.GetMigrationVersion();

                if (latestVersion != null && migrationVersion.CompareTo(latestVersion) <= 0)
                    continue;

                if (foundMigrationTypes.TryGetValue(migrationVersion, out var existingMigration))
                    throw new MigrationException($"The migration \"{type}\" and \"{existingMigration}\" have the same version \"{migrationVersion}\".");

                foundMigrationTypes.Add(migrationVersion, type);
            }
        }
    }
}