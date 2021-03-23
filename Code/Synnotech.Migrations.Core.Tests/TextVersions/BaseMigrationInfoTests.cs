using System;
using FluentAssertions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.TextVersions
{
    public static class BaseMigrationInfoTests
    {
        [Theory]
        [MemberData(nameof(CompareData))]
        public static void CompareWithMigrationVersionAttribute(MigrationInfoStub migrationInfo, MigrationVersionAttribute migrationVersion, int expectedResult) =>
            migrationInfo.CompareTo(migrationVersion).Should().Be(expectedResult);

        public static readonly TheoryData<MigrationInfoStub, MigrationVersionAttribute, int> CompareData =
            new()
            {
                { new MigrationInfoStub { Version = "0.1.0" }, new MigrationVersionAttribute("0.2.0"), -1 },
                { new MigrationInfoStub { Version = "1.0.0" }, new MigrationVersionAttribute("0.89.9"), 1 },
                { new MigrationInfoStub { Version = "14.0.3" }, new MigrationVersionAttribute("14.0.3"), 0 }
            };

        [Fact]
        public static void AttributeIsNull()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Action act = () => new MigrationInfoStub().CompareTo(null!);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("other");
        }

        [Theory]
        [MemberData(nameof(VersionData))]
        public static void VersionFieldCount(MigrationInfoStub migrationInfo, string? expectedVersion)
        {
            migrationInfo.Version.Should().Be(expectedVersion);
        }

        public static readonly TheoryData<MigrationInfoStub, string?> VersionData =
            new()
            {
                { new MigrationInfoStub { Version = "2.3.4" }, "2.3.4" },
                { new MigrationInfoStub(1) { Version = "1.0.0" }, "1" },
                { new MigrationInfoStub(2) { Version = "1.5.3" }, "1.5" },
                { new MigrationInfoStub(2) { Version = "1.5.9" }, "1.5" },
                { new MigrationInfoStub(4) { Version = "2.0.103.2203" }, "2.0.103.2203" }
            };

        [Theory]
        [InlineData("1.0.0")]
        [InlineData("2.7.1")]
        [InlineData("3.5.1")]
        public static void GetInternalVersion(string version)
        {
            var migrationInfo = new MigrationInfoStub { Version = version };

            var result = migrationInfo.TryGetInternalVersion(out var internalVersion);

            result.Should().BeTrue();
            var expectedVersion = new Version(version);
            internalVersion.Should().Be(expectedVersion);
        }

        [Fact]
        public static void InternalVersionNotAvailable()
        {
            var result = new MigrationInfoStub().TryGetInternalVersion(out var internalVersion);

            result.Should().BeFalse();
            internalVersion.Should().BeNull();
        }

        [Theory]
        [InlineData("0.3.1")]
        [InlineData("1.12.4")]
        [InlineData("16.5.6")]
        public static void SetInternalVersion(string versionText)
        {
            var migrationInfo = new MigrationInfoStub();
            var version = new Version(versionText);

            migrationInfo.SetInternalVersion(version);

            migrationInfo.Version.Should().Be(versionText);
        }

        [Fact]
        public static void SetInternalVersionToNull()
        {
            var migrationInfo = new MigrationInfoStub();

            Action act = () => migrationInfo.SetInternalVersion(null!);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("version");
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\r\n")]
        [InlineData("not really a version")]
        public static void InvalidVersion(string? version)
        {
            var migrationInfo = new MigrationInfoStub();

            Action act = () => migrationInfo.Version = version;

            act.Should().Throw<ArgumentException>();
        }

        public sealed class MigrationInfoStub : BaseMigrationInfo
        {
            public MigrationInfoStub(int fieldCount = 3) : base(fieldCount) { }
        }
    }
}