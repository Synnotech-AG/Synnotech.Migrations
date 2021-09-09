using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Synnotech.DatabaseAbstractions.Mocks;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.Int64TimestampVersions
{
    public sealed class MigrationEngineTests
    {
        private TestContext Context { get; } = new ();
        private DateTime UtcNow { get; } = DateTime.UtcNow;
        private TestMigrationInfoFactory MigrationInfoFactory { get; } = new ();

        [Fact]
        public async Task ExecuteAllMigrations()
        {
            var summary = await RunMigrationAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<TestMigrationInfo>
            {
                new () { Id = 1, Version = 20201215083000L, Name = nameof(Migration1), AppliedAt = UtcNow },
                new () { Id = 2, Version = 20210109081500L, Name = nameof(Migration2), AppliedAt = UtcNow },
                new () { Id = 3, Version = 20210217163700L, Name = nameof(Migration3), AppliedAt = UtcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
            Context.SaveChangesMustHaveBeenCalled(3)
                   .MustBeDisposed();
        }

        [Fact]
        public async Task ExecuteMigrationsWithHigherVersion()
        {
            MigrationInfoFactory.Count = 2;
            Context.LatestMigrationInfo = new TestMigrationInfo { Id = 2, Version = 20210109081500L, Name = nameof(Migration2), AppliedAt = UtcNow.AddDays(-2) };

            var summary = await RunMigrationAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<TestMigrationInfo>
            {
                new () { Id = 3, Version = 20210217163700L, Name = nameof(Migration3), AppliedAt = UtcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
            Context.SaveChangesMustHaveBeenCalled()
                   .MustBeDisposed();
        }

        [Fact]
        public async Task AllMigrationsPresent()
        {
            Context.LatestMigrationInfo = new TestMigrationInfo { Id = 3, Version = 20210217163700L, Name = nameof(Migration3), AppliedAt = UtcNow.AddMinutes(-120) };

            var summary = await RunMigrationAsync();
            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNullOrEmpty();
            Context.SaveChangesMustNotHaveBeenCalled()
                   .MustBeDisposed();
        }

        private Task<MigrationSummary<TestMigrationInfo>> RunMigrationAsync()
        {
            var engine = new MigrationEngine<TestMigration, TestMigrationInfo, TestContext>(new TestSessionFactory(Context), new ActivatorMigrationFactory<TestMigration>(), MigrationInfoFactory.Create);
            return engine.MigrateAsync(UtcNow, new[] { typeof(MigrationEngineTests).Assembly });
        }

        public abstract class TestMigration : BaseMigration, IMigration<TestContext>
        {
            public Task ApplyAsync(TestContext context, CancellationToken cancellationToken = default)
            {
                context.Should().NotBeNull(nameof(context));
                return Task.CompletedTask;
            }
        }

        [MigrationVersion("2020-12-15T08:30Z")]
        public sealed class Migration1 : TestMigration { }

        [MigrationVersion("2021-01-09T08:15Z")]
        public sealed class Migration2 : TestMigration { }

        [MigrationVersion("2021-02-17T16:37Z")]
        public sealed class Migration3 : TestMigration { }

        public sealed class TestContext : AsyncSessionMock,
                                          IGetLatestMigrationInfoSession<TestMigrationInfo>,
                                          IMigrationSession<TestContext, TestMigrationInfo>
        {
            public TestMigrationInfo? LatestMigrationInfo;
            public List<TestMigrationInfo> StoredMigrationInfos = new ();

            public Task<TestMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult(LatestMigrationInfo);

            public TestContext Context => this;

            public ValueTask StoreMigrationInfoAsync(TestMigrationInfo migrationInfo, CancellationToken cancellationToken = default)
            {
                StoredMigrationInfos.Add(migrationInfo);
                return default;
            }

            public TestContext SaveChangesMustHaveBeenCalled(int times)
            {
                SaveChangesCallCount.Should().Be(times);
                return this;
            }
        }

        public sealed class TestMigrationInfoFactory
        {
            public int Count;

            public TestMigrationInfo Create(TestMigration migration, DateTime appliedAtUtc)
            {
                return new ()
                {
                    Id = ++Count,
                    Version = migration.Version,
                    Name = migration.Name,
                    AppliedAt = appliedAtUtc
                };
            }
        }

        public sealed class TestMigrationInfo : BaseMigrationInfo
        {
            public int Id { get; set; }
        }

        public sealed class TestSessionFactory : ISessionFactory<TestMigrationInfo, TestMigration, TestContext>
        {
            private readonly TestContext _testContext;

            public TestSessionFactory(TestContext testContext) => _testContext = testContext;

            public ValueTask<IGetLatestMigrationInfoSession<TestMigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken) => new (_testContext);

            public ValueTask<IMigrationSession<TestContext, TestMigrationInfo>> CreateSessionForMigrationAsync(TestMigration migration, CancellationToken cancellationToken) => new (_testContext);
        }
    }
}