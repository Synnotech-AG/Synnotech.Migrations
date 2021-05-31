using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests
{
    public static class MigrationSummaryTests
    {
        [Fact]
        public static void ToStringWhenError()
        {
            var summary = new MigrationSummary<int>(new MigrationError<int>(1, new Exception()), null);

            summary.ToString().Should().Be("Error occurred at migration 1");
        }

        [Fact]
        public static void ToStringWithAppliedMigrations()
        {
            var summary = new MigrationSummary<int>(new List<int> { 1, 2, 3 });

            summary.ToString().Should().Be("Migrations applied");
        }

        [Fact]
        public static void ToStringWithEmptySummary()
        {
            var summary = MigrationSummary<int>.Empty;

            summary.ToString().Should().Be("No migrations applied");
        }

        [Theory]
        [InlineData(4, new[] { 1, 2, 3 })]
        [InlineData(null, new[] { 1, 2, 3, 4, 5 })]
        public static void CheckEquality(int? error, int[] appliedMigrations)
        {
            var first = CreateSummary(error, appliedMigrations);
            var second = CreateSummary(error, appliedMigrations);

            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());

            static MigrationSummary<int> CreateSummary(int? error, int[] appliedMigrations) =>
                error == null ?
                    new MigrationSummary<int>(new List<int>(appliedMigrations)) :
                    new MigrationSummary<int>(new MigrationError<int>(error.Value, new Exception()), new List<int>(appliedMigrations));
        }

        [Theory]
        [MemberData(nameof(InequalityData))]
        public static void CheckInequality(MigrationSummary<int> first, MigrationSummary<int> second) =>
            first.Should().NotBe(second);

        public static readonly TheoryData<MigrationSummary<int>, MigrationSummary<int>> InequalityData =
            new ()
            {
                { new MigrationSummary<int>(new List<int> { 1, 2, 3 }), new MigrationSummary<int>(new List<int> { 2, 3, 4 }) },
                { new MigrationSummary<int>(new MigrationError<int>(1, new Exception()), null), new MigrationSummary<int>() }
            };
    }
}