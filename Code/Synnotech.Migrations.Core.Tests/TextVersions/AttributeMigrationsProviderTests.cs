using System;
using System.Collections.Generic;
using FluentAssertions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.TextVersions
{
    public sealed class AttributeMigrationsProviderTests
    {
        private readonly AttributeMigrationsProvider<MigrationDummy, MigrationInfoDummy> _attributeMigrationsProvider;

        public AttributeMigrationsProviderTests()
        {
            _attributeMigrationsProvider = new AttributeMigrationsProvider<MigrationDummy, MigrationInfoDummy>();
        }

        [Fact]
        public void GetAllMigrations()
        {
            var migrations = DetermineMigrations(null);

            var expectedMigrations = new List<MigrationDummy>
            {
                new Migration1(),
                new Migration2(),
                new Migration3()
            };

            migrations.Should().BeEquivalentTo(expectedMigrations, config => config.WithStrictOrdering());
        }

        [Theory]
        [MemberData(nameof(GetNewestMigrationsData))]
        public void GetNewestMigrations(string version, List<MigrationDummy>? expectedMigrations)
        {
            var migrations = DetermineMigrations(new MigrationInfoDummy { Version = version });

            migrations.Should().BeEquivalentTo(expectedMigrations, config => config.WithStrictOrdering());
        }

        public static readonly TheoryData<string, List<MigrationDummy>?> GetNewestMigrationsData =
            new()
            {
                { "0.15.0", new List<MigrationDummy> { new Migration2(), new Migration3() } },
                { "0.15.4", new List<MigrationDummy> { new Migration3() } },
                { "1.0.4", null },
                { "0.17.9", new List<MigrationDummy> { new Migration3() } },
                { "1.0.5", null },
                { "2.3.0", null },
                { "0.3.1", new List<MigrationDummy> { new Migration1(), new Migration2(), new Migration3() } }
            };

        private List<MigrationDummy>? DetermineMigrations(MigrationInfoDummy? latestMigrationInfo) =>
            _attributeMigrationsProvider.DetermineMigrations(typeof(AttributeMigrationsProviderTests).Assembly, latestMigrationInfo);

        public abstract class MigrationDummy : BaseMigration<MigrationDummy> { }

        [MigrationVersion("0.15.0")]
        public sealed class Migration1 : MigrationDummy { }

        [MigrationVersion("0.15.4")]
        public sealed class Migration2 : MigrationDummy { }

        [MigrationVersion("1.0.4")]
        public sealed class Migration3 : MigrationDummy { }

        public sealed class MigrationInfoDummy : BaseMigrationInfo { }

        [Fact]
        public static void ThrowOnInvalidMigrationVersion()
        {
            var migrationsProvider = new AttributeMigrationsProvider<InvalidBaseMigration, MigrationInfoDummy>();

            Action act = () => migrationsProvider.DetermineMigrations(typeof(InvalidMigration).Assembly, null);

            act.Should().Throw<ArgumentException>()
               .And.Message.Should().Be($"The specified version \"some invalid value\" of migration \"{typeof(InvalidMigration)}\" cannot be parsed.");
        }

        public abstract class InvalidBaseMigration : BaseMigration<InvalidBaseMigration> { }

        [MigrationVersion("some invalid value")]
        public sealed class InvalidMigration : InvalidBaseMigration { }
    }
}