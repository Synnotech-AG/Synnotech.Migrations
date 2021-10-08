using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class TimestampFormatterTests
    {
        [Fact]
        public static void ConvertUtcTime()
        {
            var dateTime = new DateTime(2021, 9, 21, 14, 21, 39, DateTimeKind.Utc);

            var timestamp = dateTime.ToIso8601UtcString();

            timestamp.Should().Be("2021-09-21T14:21:39Z");
        }
    }
}