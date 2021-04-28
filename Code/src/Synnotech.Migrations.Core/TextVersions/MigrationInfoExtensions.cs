using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core.TextVersions
{
    /// <summary>
    /// Provides extension methods for migration infos that derive from <see cref="BaseMigrationInfo" />.
    /// </summary>
    public static class MigrationInfoExtensions
    {
        /// <summary>
        /// Gets the migration info with the highest version from the specified list.
        /// This method will return null when the list is null or empty.
        /// </summary>
        /// <typeparam name="TMigrationInfo">The migration info type you use to describe migration infos that derives from <see cref="BaseMigrationInfo" />.</typeparam>
        /// <param name="migrationInfos">The list of migration infos that is used to find the latest migration info.</param>
        /// <returns>The migration info with the latest version, or null if the list is null or empty.</returns>
        public static TMigrationInfo? GetLatestMigrationInfo<TMigrationInfo>(this List<TMigrationInfo>? migrationInfos)
            where TMigrationInfo : BaseMigrationInfo
        {
            if (migrationInfos.IsNullOrEmpty())
                return null;

            var sortedInfos = new SortedList<Version, TMigrationInfo>(migrationInfos.Count);
            for (var i = 0; i < migrationInfos.Count; i++)
            {
                var migrationInfo = migrationInfos[i];
                if (migrationInfo.TryGetInternalVersion(out var version))
                    sortedInfos.Add(version, migrationInfo);
            }

            var lastKey = sortedInfos.Keys[sortedInfos.Keys.Count - 1];
            return sortedInfos[lastKey];
        }
    }
}