using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public static class MigrationVersionAttributeTests
    {
        [Fact]
        public static void MustDeriveFromAttribute() =>
            typeof(MigrationVersionAttribute).Should().BeDerivedFrom<Attribute>();

        [Fact]
        public static void MustImplementIMigrationVersionAttribute() =>
            typeof(MigrationVersionAttribute).Should().Implement<IMigrationAttribute>();

        [Theory]
        [InlineData(20210902082700)]
        [InlineData(20180531163135)]
        public static void InstantiateAttribute(long version)
        {
            var attribute = new MigrationVersionAttribute(version);

            attribute.Version.Should().Be(version);
            Action validate = () => attribute.Validate(typeof(MigrationVersionAttributeTests));
            validate.Should().NotThrow();
        }
    }
}