using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.TextVersions
{
    public static class PendingMigrationsTests
    {
        [Fact]
        public static void GetAllMigrations()
        {
            var migrations = DetermineMigrations(null);

            var expectedMigrations = new List<PendingMigration<Version>>
            {
                new (new Version("0.15.0"), typeof(Migration1)),
                new (new Version("0.15.4"), typeof(Migration2)),
                new (new Version("1.0.4"), typeof(Migration3))
            };
            migrations.Should().BeEquivalentTo(expectedMigrations, config => config.WithStrictOrdering());
        }

        [Theory]
        [MemberData(nameof(GetNewestMigrationsData))]
        public static void GetNewestMigrations(string version, List<PendingMigration<Version>>? expectedMigrations)
        {
            var migrations = DetermineMigrations(new Version(version));

            migrations.Should().BeEquivalentTo(expectedMigrations, config => config.WithStrictOrdering());
        }

        public static readonly TheoryData<string, List<PendingMigration<Version>>?> GetNewestMigrationsData =
            new ()
            {
                { "0.15.0", new List<PendingMigration<Version>>(2) { typeof(Migration2).ToPendingMigration(), typeof(Migration3).ToPendingMigration() } },
                { "0.15.4", new List<PendingMigration<Version>>(1) { typeof(Migration3).ToPendingMigration() } },
                { "1.0.4", null },
                { "0.17.9", new List<PendingMigration<Version>>(1) { typeof(Migration3).ToPendingMigration() } },
                { "1.0.5", null },
                { "2.3.0", null },
                { "0.3.1", new List<PendingMigration<Version>>(3) { typeof(Migration1).ToPendingMigration(), typeof(Migration2).ToPendingMigration(), typeof(Migration3).ToPendingMigration() } }
            };

        private static List<PendingMigration<Version>>? DetermineMigrations(Version? latestVersion) =>
            PendingMigrations.DetermineNewMigrations<Version, MigrationDummy, MigrationVersionAttribute>(latestVersion, typeof(PendingMigrationsTests).Assembly);

        public abstract class MigrationDummy : BaseMigration { }

        [MigrationVersion("0.15.0")]
        public sealed class Migration1 : MigrationDummy { }

        [MigrationVersion("0.15.4")]
        public sealed class Migration2 : MigrationDummy { }

        [MigrationVersion("1.0.4")]
        public sealed class Migration3 : MigrationDummy { }

        [Fact]
        public static void ThrowOnInvalidMigrationVersion()
        {
            Action act = () => PendingMigrations.DetermineNewMigrations<Version, InvalidBaseMigration, MigrationVersionAttribute>(null, typeof(InvalidMigration).Assembly);

            act.Should().Throw<MigrationException>()
               .And.Message.Should().Be($"The specified version \"some invalid value\" of migration \"{typeof(InvalidMigration)}\" cannot be parsed.");
        }

        public abstract class InvalidBaseMigration : BaseMigration { }

        [MigrationVersion("some invalid value")]
        public sealed class InvalidMigration : InvalidBaseMigration { }

        private static PendingMigration<Version> ToPendingMigration(this Type migrationType)
        {
            var migrationVersionAttribute = migrationType.GetCustomAttribute<MigrationVersionAttribute>()!;
            return new (migrationVersionAttribute.Version, migrationType);
        }
    }
}