using FluentAssertions;
using Xunit;

namespace Synnotech.SqlServer.Tests
{
    public static class EscapeDatabaseNameTests
    {
        [Theory]
        [InlineData("A", "A")]
        [InlineData("B", "B")]
        [InlineData("  C", "C")]
        [InlineData("D\t", "D")]
        [InlineData("\r\ne\t", "e")]
        public static void ValidDatabaseNames(string validName, string expected)
        {
            SqlEscaping.CheckAndNormalizeDatabaseName(validName).Should().Be(expected);
        }
    }
}