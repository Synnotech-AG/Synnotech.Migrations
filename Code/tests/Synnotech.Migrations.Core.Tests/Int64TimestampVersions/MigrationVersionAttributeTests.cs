using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public sealed class MigrationVersionAttributeTests
    {
        public MigrationVersionAttributeTests(ITestOutputHelper output) => Output = output;

        private ITestOutputHelper Output { get; }

        [Fact]
        public static void MustDeriveFromAttribute() =>
            typeof(MigrationVersionAttribute).Should().BeDerivedFrom<Attribute>();

        [Fact]
        public static void MustImplementIMigrationVersionAttribute() =>
            typeof(MigrationVersionAttribute).Should().Implement<IMigrationAttribute>();

        [Theory]
        [InlineData(20210902082700)]
        [InlineData(20180531163135)]
        public static void ValidInt64ValueShouldNotThrowOnValidate(long version)
        {
            var attribute = new MigrationVersionAttribute(version);

            attribute.Version.Should().Be(version);
            Action validate = () => attribute.Validate(typeof(MigrationVersionAttributeTests));
            validate.Should().NotThrow();
        }

        [Theory]
        [InlineData("2021-09-08T08:01Z", 20210908080100L)]
        [InlineData("2018-02-28T19:31:48Z", 20180228193148L)]
        public static void ValidIso8601TimestampShouldNotThrow(string iso8601Timestamp, long expectedVersion)
        {
            var attribute = new MigrationVersionAttribute(iso8601Timestamp);

            attribute.Version.Should().Be(expectedVersion);
            Action act = () => attribute.Validate(typeof(MigrationVersionAttributeTests));
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("Foo")]
        [InlineData("2021-49-27T58:02Z")]
        public void InvalidIso8601TimestampShouldThrowOnValidate(string invalidTimestamp)
        {
            var attribute = new MigrationVersionAttribute(invalidTimestamp);

            Action act = () => attribute.Validate(typeof(MigrationVersionAttributeTests));

            act.Should().Throw<MigrationException>().Which.ShouldBeWrittenTo(Output);
        }
    }
}