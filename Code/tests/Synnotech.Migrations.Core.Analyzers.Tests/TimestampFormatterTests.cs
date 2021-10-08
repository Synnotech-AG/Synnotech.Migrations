using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class TimestampFormatterTests
    {
        [Fact]
        public static void ConvertToIso8601UtcString()
        {
            var dateTime = new DateTime(2021, 9, 21, 14, 21, 39, DateTimeKind.Utc);

            var timestamp = dateTime.ToIso8601UtcString();

            timestamp.Should().Be("2021-09-21T14:21:39Z");
        }

        [Fact]
        public static void ConvertToInt64Timestamp()
        {
            var dateTime = new DateTime(2021, 10, 8, 15, 2, 38, DateTimeKind.Utc);

            var timestamp = dateTime.ToInt64Timestamp();

            timestamp.Should().Be(20211008150238);
        }
    }
}