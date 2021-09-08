using System;
using FluentAssertions;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public static class BaseMigrationTests
    {
        [Theory]
        [MemberData(nameof(EqualityData))]
        public static void Equality(TestMigration x, TestMigration? y, bool expected) =>
            x.Equals(y!).Should().Be(expected);

        public static readonly TheoryData<TestMigration, TestMigration?, bool> EqualityData =
            new ()
            {
                { new Migration1(), new Migration2(), false },
                { new Migration3(), new Migration3(), true },
                { new Migration3(), new Migration1(), false },
                { Migration1.Instance, Migration1.Instance, true },
                { new Migration2(), null, false }
            };

        [Fact]
        public static void MissingAttribute()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new MigrationWithoutAttribute();

            act.Should().Throw<InvalidOperationException>()
               .And.Message.Should().Be($"The {nameof(MigrationVersionAttribute)} is not applied to migration \"{typeof(MigrationWithoutAttribute)}\".");
        }

        [Theory]
        [MemberData(nameof(HashCodeData))]
        public static void HashCode(TestMigration migration) =>
            migration.GetHashCode().Should().Be(migration.Version.GetHashCode());

        public static readonly TheoryData<TestMigration> HashCodeData =
            new ()
            {
                new Migration1(),
                new Migration2(),
                new Migration3()
            };

        public abstract class TestMigration : BaseMigration { }

        [MigrationVersion("2021-09-08T08:00Z")]
        public sealed class Migration1 : TestMigration
        {
            public static Migration1 Instance = new ();
        }

        [MigrationVersion("2021-09-08T08:15Z")]
        public sealed class Migration2 : TestMigration { }

        [MigrationVersion("2021-09-10T15:19:33Z")]
        public sealed class Migration3 : TestMigration { }

        public sealed class MigrationWithoutAttribute : TestMigration { }
    }
}