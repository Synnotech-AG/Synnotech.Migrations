using System;
using FluentAssertions;
using Light.GuardClauses.FrameworkExtensions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.TextVersions
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
        [InlineData("1.0.0")]
        [InlineData("0.5.9")]
        [InlineData("19.3.0")]
        public static void ValidVersion(string version)
        {
            var attribute = new MigrationVersionAttribute(version);

            var expectedVersion = new Version(version);
            attribute.Version.Should().Be(expectedVersion);
            Action validate = () => attribute.Validate(typeof(MigrationVersionAttributeTests));
            validate.Should().NotThrow();
        }

        [Theory]
        [InlineData("invalid data")]
        [InlineData("foo")]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData(null)]
        public static void InvalidVersion(string invalidVersion)
        {
            var attribute = new MigrationVersionAttribute(invalidVersion);

            Action getVersion = () =>
            {
                var _ = attribute.Version;
            };
            Action validate = () => attribute.Validate(typeof(MigrationVersionAttributeTests));

            getVersion.Should().Throw<ArgumentException>()
                      .And.Message.Should().Be($"The specified version {invalidVersion.ToStringOrNull()} cannot be parsed.");
            validate.Should().Throw<MigrationException>()
                    .And.Message.Should().Be($"The specified version {invalidVersion.ToStringOrNull()} of migration \"{typeof(MigrationVersionAttributeTests)}\" cannot be parsed.");
        }
    }
}