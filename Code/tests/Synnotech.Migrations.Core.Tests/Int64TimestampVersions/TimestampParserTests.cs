using FluentAssertions;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public static class TimestampParserTests
    {
        [Theory]
        [InlineData("2021-09-06T16:45Z", 20210906164500L)]
        [InlineData("2019-12-24T17:00Z", 20191224170000L)]
        [InlineData("1987-02-12T10:32:56Z", 19870212103256L)]
        [InlineData("1548-05-07T04:07:03Z", 15480507040703L)]
        public static void ParseValidIso8601String(string value, long expected)
        {
            var result = TimestampParser.TryParseTimestamp(value, out var int64Timestamp);

            result.Should().BeTrue();
            int64Timestamp.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Foo")]
        [InlineData("1")]
        [InlineData("19")]
        [InlineData("192")]
        [InlineData("1999")]
        [InlineData("1999-0")]
        [InlineData("200013-0")]
        [InlineData("2021-05")]
        [InlineData("2020-11-17")]
        [InlineData("2021-12-20-")]
        [InlineData("2018-08-20T")]
        [InlineData("2018-08-20T17:")]
        [InlineData("2012-08-20T15:00")]
        [InlineData("2018.08.20T15.00Z")]
        [InlineData("Some String that is way too long")]
        public static void InvalidText(string value)
        {
            var result = TimestampParser.TryParseTimestamp(value, out var int64Timestamp);

            result.Should().BeFalse();
            int64Timestamp.Should().Be(0L);
        }
    }
}