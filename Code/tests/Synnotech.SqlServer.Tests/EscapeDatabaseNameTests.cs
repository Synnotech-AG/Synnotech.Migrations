using System;
using FluentAssertions;
using Synnotech.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.SqlServer.Tests
{
    public sealed class EscapeDatabaseNameTests
    {
        public EscapeDatabaseNameTests(ITestOutputHelper output) => Output = output;

        private ITestOutputHelper Output { get; }

        [Theory]
        [InlineData("A", "A")]
        [InlineData("B", "B")]
        [InlineData("  C", "C")]
        [InlineData("D\t", "D")]
        [InlineData("\r\ne\t", "e")]
        [InlineData("Foo", "Foo")]
        [InlineData("Update", "[Update]")]
        [InlineData("Table", "[Table]")]
        [InlineData("Table2016", "Table2016")]
        [InlineData("IUseÜmläuts", "IUseÜmläuts")]
        [InlineData("_My$Table#With@All_AllowedSigns123", "_My$Table#With@All_AllowedSigns123")]
        public static void ValidDatabaseNames(string validName, string expected) =>
            SqlEscaping.CheckAndNormalizeDatabaseName(validName).Should().Be(expected);

        [Theory]
        [InlineData("%ABC")] // Invalid first character
        [InlineData("!AB")]
        [InlineData("DatabaseName'; DROP DATABASE FOO; --")] // SQL Injection Attack
        [InlineData("")] // Empty String
        [InlineData(null)] // null
        [InlineData("\t\r\n")] // white space
        [InlineData("Invalid Name")] // White space in between
        [InlineData("Other$Invalid§Special?Characters")] // White space in between
        public void InvalidDatabaseNames(string invalidName)
        {
            Action act = () => SqlEscaping.CheckAndNormalizeDatabaseName(invalidName);

            act.Should().Throw<ArgumentException>()
               .Which.ShouldBeWrittenTo(Output);
        }
    }
}