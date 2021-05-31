using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Synnotech.Migrations.Core.Tests.TestHelpers;
using Xunit;

namespace Synnotech.Migrations.Core.Tests
{
    public static class MigrationPlanTests
    {
        private static Type DummyMigrationType { get; } = typeof(MigrationPlanTests);

        [Fact]
        public static void HasPendingMigrations()
        {
            var plan = new MigrationPlan<int, IntMigrationInfo>(
                null,
                new List<PendingMigration<int>>(1) { new (1, DummyMigrationType) }
            );

            plan.HasPendingMigrations.Should().BeTrue();
        }

        [Fact]
        public static void HasNoPendingMigrations()
        {
            var plan = new MigrationPlan<int, IntMigrationInfo>(null, null);

            plan.HasPendingMigrations.Should().BeFalse();
        }

        [Theory]
        [InlineData(new[] { 1 }, "Pending migrations")]
        [InlineData(new int[] { }, "No pending migrations")]
        public static void StringRepresentation(int[] migrationVersions, string expectedText)
        {
            var plan = new MigrationPlan<int, IntMigrationInfo>(
                null,
                migrationVersions.ConvertToPendingMigrations()
            );

            plan.ToString().Should().Be(expectedText);
        }

        [Theory]
        [InlineData(1, new[] { 2, 3, 4 })]
        [InlineData(null, new[] { 1, 2 })]
        public static void CheckEquality(int? currentVersion, int[] migrationVersions)
        {
            var pendingMigrations = migrationVersions.ConvertToPendingMigrations();
            IntMigrationInfo? migrationInfo = currentVersion;
            var first = new MigrationPlan<int, IntMigrationInfo>(migrationInfo, pendingMigrations);
            var second = new MigrationPlan<int, IntMigrationInfo>(migrationInfo, pendingMigrations.ToList());

            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(InequalityData))]
        public static void CheckInequality(MigrationPlan<int, IntMigrationInfo> first, MigrationPlan<int, IntMigrationInfo> second) =>
            first.Should().NotBe(second);

        public static readonly TheoryData<MigrationPlan<int, IntMigrationInfo>, MigrationPlan<int, IntMigrationInfo>> InequalityData =
            new ()
            {
                {
                    new MigrationPlan<int, IntMigrationInfo>(1, new[] { 2, 3, 4 }.ConvertToPendingMigrations()),
                    new MigrationPlan<int, IntMigrationInfo>(2, new[] { 3, 4 }.ConvertToPendingMigrations())
                },
                {
                    new MigrationPlan<int, IntMigrationInfo>(null, new[] { 1, 2 }.ConvertToPendingMigrations()),
                    new MigrationPlan<int, IntMigrationInfo>(1, new[] { 2, 3, 4 }.ConvertToPendingMigrations())
                }
            };

        private static List<PendingMigration<int>> ConvertToPendingMigrations(this int[] versions) =>
            versions.Select(version => new PendingMigration<int>(version, DummyMigrationType))
                    .ToList();
    }
}