using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

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
        /// <param name="assembliesContainingMigrations">
        /// The assemblies that will be searched for migration types (optional). If you do not provide any assemblies,
        /// the calling assembly will be searched.
        /// </param>
        /// <returns>A list of pending migrations whose version is higher than the specified migration version.</returns>
        /// <exception cref="MigrationException">Thrown when any migration type is found whose migration attribute is invalid.</exception>
        public static List<PendingMigration<TMigrationVersion>>? DetermineNewMigrations<TMigrationVersion, TMigration, TMigrationAttribute>(this Assembly[] assembliesContainingMigrations,
                                                                                                                                            TMigrationVersion? latestVersion)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        {
            if (assembliesContainingMigrations.IsNullOrEmpty())
                assembliesContainingMigrations = new[] { Assembly.GetCallingAssembly() };

            var migrationBaseType = typeof(TMigration);
            var foundMigrationTypes = new Dictionary<TMigrationVersion, Type>();
            foreach (var assembly in assembliesContainingMigrations)
            {
                FindNewerMigrationTypes<TMigrationVersion, TMigrationAttribute>(assembly, latestVersion, migrationBaseType, foundMigrationTypes);
            }

            return foundMigrationTypes.ConvertToPendingMigrations();
        }

        private static void FindNewerMigrationTypes<TMigrationVersion, TMigrationAttribute>(Assembly assembly,
                                                                                            TMigrationVersion? latestVersion,
                                                                                            Type migrationBaseType,
                                                                                            Dictionary<TMigrationVersion, Type> foundMigrationTypes)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (!type.CheckIfTypeIsMigration<TMigrationAttribute>(migrationBaseType, out var migrationAttribute))
                    continue;

                var migrationVersion = migrationAttribute.GetMigrationVersion();
                if (latestVersion != null && migrationVersion.CompareTo(latestVersion) <= 0)
                    continue;

                foundMigrationTypes.AddWithUniquenessValidation(migrationVersion, type);
            }
        }

        /// <summary>
        /// Searches the specified assemblies for types that represent migrations and are not in the list of applied migrations.
        /// </summary>
        /// <typeparam name="TMigrationVersion">The type that represents a migration version.</typeparam>
        /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
        /// <typeparam name="TMigrationAttribute">The type that represents the attribute being applied to migrations to indicate their version.</typeparam>
        /// <param name="appliedVersions"></param>
        /// <param name="assembliesContainingMigrations">
        /// The assemblies that will be searched for migration types (optional). If you do not provide any assemblies,
        /// the calling assembly will be searched.
        /// </param>
        /// <returns>A list of pending migrations that are not part of the list of applied migrations.</returns>
        /// <exception cref="MigrationException">Thrown when any migration type is found whose migration attribute is invalid.</exception>
        public static List<PendingMigration<TMigrationVersion>>? DetermineNonAppliedMigrations<TMigrationVersion, TMigration, TMigrationAttribute>(this Assembly[] assembliesContainingMigrations,
                                                                                                                                                   HashSet<TMigrationVersion> appliedVersions)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        {
            if (assembliesContainingMigrations.IsNullOrEmpty())
                assembliesContainingMigrations = new[] { Assembly.GetCallingAssembly() };

            var migrationBaseType = typeof(TMigration);
            var foundMigrationTypes = new Dictionary<TMigrationVersion, Type>();
            foreach (var assembly in assembliesContainingMigrations)
            {
                FindNonAppliedMigrationTypes<TMigrationVersion, TMigrationAttribute>(assembly, appliedVersions, migrationBaseType, foundMigrationTypes);
            }

            return foundMigrationTypes.ConvertToPendingMigrations();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMigrationInfo"></typeparam>
        /// <typeparam name="TMigrationVersion"></typeparam>
        /// <param name="migrationInfos"></param>
        /// <returns></returns>
        public static (TMigrationInfo? infoWithHighestVersion, HashSet<TMigrationVersion> allVersions) ExtractMigrationVersions<TMigrationInfo, TMigrationVersion>(this List<TMigrationInfo> migrationInfos)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationInfo : class, IHasMigrationVersion<TMigrationVersion>
        {
            var hashSet = new HashSet<TMigrationVersion>();
            if (migrationInfos.IsNullOrEmpty())
                return (null, hashSet);

            var infoWithHighestVersion = migrationInfos[0];
            for (var i = 0; i < migrationInfos.Count; i++)
            {
                var migrationInfo = migrationInfos[i];
                var migrationVersion = migrationInfo.GetMigrationVersion();
                if (infoWithHighestVersion.GetMigrationVersion().CompareTo(migrationVersion) < 0)
                    infoWithHighestVersion = migrationInfo;

                if (!hashSet.Add(migrationVersion))
                    throw new MigrationException($"The migration version {migrationVersion.ToStringOrNull()} occurs several times in the list of migration infos.");
            }

            return (infoWithHighestVersion, hashSet);
        }

        private static void FindNonAppliedMigrationTypes<TMigrationVersion, TMigrationAttribute>(Assembly assembly,
                                                                                                 HashSet<TMigrationVersion> appliedVersions,
                                                                                                 Type migrationBaseType,
                                                                                                 Dictionary<TMigrationVersion, Type> foundMigrationTypes)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
            where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (!type.CheckIfTypeIsMigration<TMigrationAttribute>(migrationBaseType, out var migrationAttribute))
                    continue;

                var migrationVersion = migrationAttribute.GetMigrationVersion();
                if (appliedVersions.Contains(migrationVersion))
                    continue;

                foundMigrationTypes.AddWithUniquenessValidation(migrationVersion, type);
            }
        }

        private static void AddWithUniquenessValidation<TMigrationVersion>(this Dictionary<TMigrationVersion, Type> foundMigrationTypes,
                                                                           TMigrationVersion migrationVersion,
                                                                           Type type)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        {
            if (foundMigrationTypes.TryGetValue(migrationVersion, out var existingMigration))
                throw new MigrationException($"The migration \"{type}\" and \"{existingMigration}\" have the same version \"{migrationVersion}\".");

            foundMigrationTypes.Add(migrationVersion, type);
        }

        private static List<PendingMigration<TMigrationVersion>>? ConvertToPendingMigrations<TMigrationVersion>(this Dictionary<TMigrationVersion, Type> foundMigrationTypes)
            where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        {
            return foundMigrationTypes.Count switch
            {
                0 => null,
                1 => new List<PendingMigration<TMigrationVersion>>(1) { PendingMigration<TMigrationVersion>.FromKeyValuePair(foundMigrationTypes.First()) },
                _ => foundMigrationTypes.OrderBy(keyValuePair => keyValuePair.Key)
                                        .Select(PendingMigration<TMigrationVersion>.FromKeyValuePair)
                                        .ToList()
            };
        }
    }
}