using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests
{
    public static class MigrationPlanTests
    {
        [Theory]
        [InlineData(1, new[] { 2, 3, 4 })]
        [InlineData(null, new[] { 1, 2 })]
        public static void CheckEquality(int? currentVersion, int[] migrationsToBeApplied)
        {
            var first = new MigrationPlan<int, int?>(currentVersion, new List<int>(migrationsToBeApplied));
            var second = new MigrationPlan<int, int?>(currentVersion, new List<int>(migrationsToBeApplied));

            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(InequalityData))]
        public static void CheckInequality(MigrationPlan<int, int?> first, MigrationPlan<int, int?> second)
        {
            first.Should().NotBe(second);
        }

        public static readonly TheoryData<MigrationPlan<int, int?>, MigrationPlan<int, int?>> InequalityData =
            new()
            {
                { new MigrationPlan<int, int?>(1, new List<int> { 2, 3, 4 }), new MigrationPlan<int, int?>(2, new List<int> { 3, 4 }) },
                { new MigrationPlan<int, int?>(null, new List<int> { 1, 2 }), new MigrationPlan<int, int?>(1, new List<int> { 2, 3, 4 }) }
            };
    }
}