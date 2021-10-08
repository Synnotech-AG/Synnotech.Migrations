using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public static class BaseMigrationInfoTests
    {
        [Fact]
        public static void MustImplementIHasMigrationVersion() =>
            typeof(BaseMigrationInfo).Should().Implement<IHasMigrationVersion<long>>();

        [Theory]
        [InlineData(1L)]
        [InlineData(20210908075300L)]
        public static void GetMigrationVersion(long version)
        {
            var migrationInfo = new MigrationInfoStub { Version = version };

            migrationInfo.Version.Should().Be(version);
            ((IHasMigrationVersion<long>)migrationInfo).GetMigrationVersion().Should().Be(version);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public static void InvalidName(string? name)
        {
            var migrationInfo = new MigrationInfoStub();
            Action act = () => migrationInfo.Name = name;

            act.Should().Throw<ArgumentException>();
        }

        private sealed class MigrationInfoStub : BaseMigrationInfo { }
    }
}