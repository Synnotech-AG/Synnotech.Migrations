using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests
{
    public sealed class AsyncMigrationEngineTests
    {
        private readonly TestContext _context = new();
        private readonly DateTime _utcNow = DateTime.UtcNow;
        private readonly TestMigrationInfoFactory _migrationInfoFactory = new();

        [Fact]
        public async Task ExecuteAllMigrations()
        {
            var summary = await RunMigrationAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<TestMigrationInfo>
            {
                new() { Id = 1, Version = "0.1.0", Name = nameof(Migration1), AppliedAt = _utcNow },
                new() { Id = 2, Version = "0.2.0", Name = nameof(Migration2), AppliedAt = _utcNow },
                new() { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = _utcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
            _context.MustBeCommittedAndDisposed();
        }

        [Fact]
        public async Task ExecuteMigrationsWithHigherVersion()
        {
            _migrationInfoFactory.Count = 2;
            _context.LatestMigrationInfo = new TestMigrationInfo { Id = 2, Version = "0.2.0", Name = nameof(Migration2), AppliedAt = _utcNow.AddDays(-2) };

            var summary = await RunMigrationAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<TestMigrationInfo>
            {
                new() { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = _utcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
            _context.MustBeCommittedAndDisposed();
        }

        [Fact]
        public async Task AllMigrationsPresent()
        {
            _context.LatestMigrationInfo = new TestMigrationInfo { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = _utcNow.AddMinutes(-120) };

            var summary = await RunMigrationAsync();
            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNullOrEmpty();
            summary.AppliedMigrations.Should().BeNullOrEmpty();
            _context.MustBeRolledBackAndDisposed();
        }

        private Task<MigrationSummary<TestMigrationInfo>> RunMigrationAsync()
        {
            var migrationsProvider = new AttributeMigrationsProvider<TestMigration, TestMigrationInfo>();
            var engine = new AsyncMigrationEngine<TestContext, TestMigration, TestMigrationInfo>(new TestAsyncSessionFactory(_context), migrationsProvider, _migrationInfoFactory.Create);
            return engine.MigrateAsync(typeof(AsyncMigrationEngineTests).Assembly, _utcNow);
        }

        public abstract class TestMigration : BaseMigration<TestMigration>, IAsyncMigration<TestContext>
        {
            public Task ApplyAsync(TestContext context) => Task.CompletedTask;
        }

        [MigrationVersion("0.1.0")]
        public sealed class Migration1 : TestMigration { }

        [MigrationVersion("0.2.0")]
        public sealed class Migration2 : TestMigration { }

        [MigrationVersion("1.0.0")]
        public sealed class Migration3 : TestMigration { }

        public sealed class TestMigrationInfo : BaseMigrationInfo
        {
            public int Id { get; set; }
            public DateTime AppliedAt { get; set; }
        }

        public sealed class TestContext : IAsyncMigrationSession<TestMigrationInfo>
        {
            public int DisposeCallCount;
            public TestMigrationInfo? LatestMigrationInfo;
            public int SaveChangesCallCount;
            public List<TestMigrationInfo> StoredMigrationInfos = new();

            public void Dispose() => DisposeCallCount++;

            public Task<TestMigrationInfo?> GetLatestMigrationInfoAsync() =>
                Task.FromResult(LatestMigrationInfo);

            public Task StoreMigrationInfoAsync(TestMigrationInfo migrationInfo)
            {
                StoredMigrationInfos.Add(migrationInfo);
                return Task.CompletedTask;
            }

            public Task SaveChangesAsync()
            {
                SaveChangesCallCount++;
                return Task.CompletedTask;
            }

            public void MustBeDisposed() => DisposeCallCount.Should().BeGreaterOrEqualTo(1, "Dispose must be called at least once");

            public void MustBeCommitted() => SaveChangesCallCount.Should().BeGreaterOrEqualTo(1, "SaveChanges must have been called.");

            public void MustBeRolledBack() => SaveChangesCallCount.Should().Be(0, "SaveChanges must not have been called.");

            public void MustBeCommittedAndDisposed()
            {
                MustBeCommitted();
                MustBeDisposed();
            }

            public void MustBeRolledBackAndDisposed()
            {
                MustBeRolledBack();
                MustBeDisposed();
            }
        }

        public sealed class TestMigrationInfoFactory
        {
            public int Count;

            public TestMigrationInfo Create(TestMigration migration, DateTime appliedAtUtc)
            {
                return new()
                {
                    Id = ++Count,
                    Version = migration.VersionString,
                    Name = migration.Name,
                    AppliedAt = appliedAtUtc
                };
            }
        }

        public sealed class TestAsyncSessionFactory : IAsyncSessionFactory<TestContext, TestMigration>
        {
            private readonly TestContext _testContext;

            public TestAsyncSessionFactory(TestContext testContext) => _testContext = testContext;

            public TestContext CreateSessionForRetrievingLatestMigrationInfo() => _testContext;

            public TestContext CreateSessionForMigration(TestMigration migration) => _testContext;
        }
    }
}