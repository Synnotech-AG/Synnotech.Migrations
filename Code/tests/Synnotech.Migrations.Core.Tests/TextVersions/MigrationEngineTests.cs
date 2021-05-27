﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Synnotech.Migrations.Core.TextVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Tests.TextVersions
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
                new () { Id = 1, Version = "0.1.0", Name = nameof(Migration1), AppliedAt = UtcNow },
                new () { Id = 2, Version = "0.2.0", Name = nameof(Migration2), AppliedAt = UtcNow },
                new () { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = UtcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
            Context.MustBeCommittedAndDisposed();
        }

        [Fact]
        public async Task ExecuteMigrationsWithHigherVersion()
        {
            MigrationInfoFactory.Count = 2;
            Context.LatestMigrationInfo = new TestMigrationInfo { Id = 2, Version = "0.2.0", Name = nameof(Migration2), AppliedAt = UtcNow.AddDays(-2) };

            var summary = await RunMigrationAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<TestMigrationInfo>
            {
                new () { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = UtcNow }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
            Context.MustBeCommittedAndDisposed();
        }

        [Fact]
        public async Task AllMigrationsPresent()
        {
            Context.LatestMigrationInfo = new TestMigrationInfo { Id = 3, Version = "1.0.0", Name = nameof(Migration3), AppliedAt = UtcNow.AddMinutes(-120) };

            var summary = await RunMigrationAsync();
            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNullOrEmpty();
            Context.MustBeRolledBackAndDisposed();
        }

        private Task<MigrationSummary<TestMigrationInfo>> RunMigrationAsync()
        {
            var engine = new MigrationEngine<TestMigration, TestMigrationInfo, TestContext>(new TestSessionFactory(Context), new ActivatorMigrationFactory<TestMigration>(), MigrationInfoFactory.Create);
            return engine.MigrateAsync(UtcNow, new[] { typeof(MigrationEngineTests).Assembly });
        }
    }

    public abstract class TestMigration : BaseMigration, IMigration<TestContext>
    {
        public Task ApplyAsync(TestContext context, CancellationToken cancellationToken)
        {
            context.Should().NotBeNull();
            return Task.CompletedTask;
        }
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
    }


    public sealed class TestContext : IGetLatestMigrationInfoSession<TestMigrationInfo>, IMigrationSession<TestContext, TestMigrationInfo>
    {
        public int DisposeCallCount;
        public TestMigrationInfo? LatestMigrationInfo;
        public int SaveChangesCallCount;
        public List<TestMigrationInfo> StoredMigrationInfos = new ();

        public void Dispose() => DisposeCallCount++;

        public Task<TestMigrationInfo?> GetLatestMigrationInfoAsync(CancellationToken cancellationToken) =>
            Task.FromResult(LatestMigrationInfo);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        public TestContext Context => this;

        public ValueTask StoreMigrationInfoAsync(TestMigrationInfo migrationInfo, CancellationToken cancellationToken)
        {
            StoredMigrationInfos.Add(migrationInfo);
            return default;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
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
            return new ()
            {
                Id = ++Count,
                Version = migration.ConvertVersionToString(),
                Name = migration.Name,
                AppliedAt = appliedAtUtc
            };
        }
    }

    public sealed class TestSessionFactory : ISessionFactory<TestMigrationInfo, TestMigration, TestContext>
    {
        private readonly TestContext _testContext;

        public TestSessionFactory(TestContext testContext) => _testContext = testContext;

        public ValueTask<IGetLatestMigrationInfoSession<TestMigrationInfo>> CreateSessionForRetrievingLatestMigrationInfoAsync(CancellationToken cancellationToken) => new (_testContext);

        public ValueTask<IMigrationSession<TestContext, TestMigrationInfo>> CreateSessionForMigrationAsync(TestMigration migration, CancellationToken cancellationToken) => new (_testContext);
    }
}