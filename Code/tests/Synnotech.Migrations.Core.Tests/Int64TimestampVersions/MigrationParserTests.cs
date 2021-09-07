using System;
using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public class MigrationParserTests
    {
        [Theory]
        [MemberData(nameof(Iso8601Data))]
        public static void ParseIso8601String(string value, DateTime expected)
        {
            var actual = DateTime.Parse(value, styles: DateTimeStyles.RoundtripKind);

            actual.Should().Be(expected);
        }

        public static readonly TheoryData<string, DateTime> Iso8601Data =
            new ()
            {
                { "2021-09-06T16:45Z", new DateTime(2021, 9, 6, 16, 45, 0, DateTimeKind.Utc) },
                { "1987-02-12T10:32:56Z", new DateTime(1987, 2, 12, 10, 32, 56, DateTimeKind.Utc) }
            };
    }
}