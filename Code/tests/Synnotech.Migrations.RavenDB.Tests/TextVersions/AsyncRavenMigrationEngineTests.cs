using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;
using Synnotech.Migrations.RavenDB.TextVersions;
using Xunit;

namespace Synnotech.Migrations.RavenDB.Tests.TextVersions
{
    public sealed class AsyncRavenMigrationEngineTests : RavenDbIntegrationTest
    {
        [SkippableFact]
        public async Task MigrateAll()
        {
            TestSettings.SkipDatabaseIntegrationTestIfNecessary();

            var now = DateTime.UtcNow;
            using var store = GetDocumentStore();
            var engine = new MigrationEngine(new SessionFactory(store));

            var summary = await engine.MigrateAsync(typeof(AsyncRavenMigrationEngineTests).Assembly, now);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new() { Id = "migrationInfos/1.0.0", Name = nameof(FirstMigration), Version = "1.0.0", AppliedAt = now },
                new() { Id = "migrationInfos/2.0.0", Name = nameof(SecondMigration), Version = "2.0.0", AppliedAt = now },
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
            using var session = store.OpenAsyncSession();
            var storedEntities = await session.Query<Entity>().ToListAsync();
            var expectedStoredEntities = new List<Entity>
            {
                new() { Id = "entities/1-A", Value = "Foo" },
                new() { Id = "entities/2-A", Value = "Bar" }
            };
            storedEntities.Should().BeEquivalentTo(expectedStoredEntities, config => config.WithStrictOrdering());
            var storedMigrationInfos = await session.Query<MigrationInfo>().ToListAsync();
            storedMigrationInfos.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
        }

        [SkippableFact]
        public async Task MigrateNewest()
        {
            TestSettings.SkipDatabaseIntegrationTestIfNecessary();

            var now = DateTime.UtcNow;
            using var store = GetDocumentStore();
            var engine = new MigrationEngine(new SessionFactory(store));
            using var session = store.OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            await session.StoreAsync(new MigrationInfo { Id = "migrationInfos/1.0.0", Name = nameof(FirstMigration), Version = "1.0.0", AppliedAt = now.AddDays(-1) });
            await session.SaveChangesAsync();

            var summary = await engine.MigrateAsync(typeof(AsyncRavenMigrationEngineTests).Assembly, now);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfo = new MigrationInfo { Id = "migrationInfos/2.0.0", Name = nameof(SecondMigration), Version = "2.0.0", AppliedAt = now };
            var expectedMigrationInfos = new List<MigrationInfo> { expectedMigrationInfo };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
            var storedEntities = await session.Query<Entity>().ToListAsync();
            var expectedStoredEntities = new List<Entity> { new() { Id = "entities/1-A", Value = "Bar" } };
            storedEntities.Should().BeEquivalentTo(expectedStoredEntities);
            var storedMigrationInfo = await session.LoadAsync<MigrationInfo?>(expectedMigrationInfo.Id);
            storedMigrationInfo.Should().BeEquivalentTo(expectedMigrationInfo);
        }

        [SkippableFact]
        public async Task AllMigrationsApplied()
        {
            TestSettings.SkipDatabaseIntegrationTestIfNecessary();

            var now = DateTime.UtcNow;
            using var store = GetDocumentStore();
            var engine = new MigrationEngine(new SessionFactory(store));
            using var session = store.OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            await session.StoreAsync(new MigrationInfo { Id = "migrationInfos/2.0.0", Name = nameof(FirstMigration), Version = "2.0.0", AppliedAt = now.AddDays(-5) });
            await session.SaveChangesAsync();

            var summary = await engine.MigrateAsync(typeof(AsyncRavenMigrationEngineTests).Assembly, now);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNullOrEmpty();
            summary.AppliedMigrations.Should().BeNullOrEmpty();
            var entities = await session.Query<Entity>().ToListAsync();
            entities.Should().BeNullOrEmpty();
        }

        [SkippableFact]
        public async Task GetMigrationPlan()
        {
            TestSettings.SkipDatabaseIntegrationTestIfNecessary();

            var now = DateTime.UtcNow;
            var services = new ServiceCollection().AddSingleton(GetDocumentStore())
                                                  .AddSynnotechMigrations();

            var migrationEngine = services.BuildServiceProvider().GetRequiredService<MigrationEngine>();
            var migrationPlan = await migrationEngine.GenerateMigrationPlanAsync(typeof(AsyncRavenMigrationEngineTests).Assembly);

            var expectedPlan = new MigrationPlan<Migration, MigrationInfo>(null, new List<Migration> { new FirstMigration(), new SecondMigration() });
            migrationPlan.Should().Be(expectedPlan);
        }

        [MigrationVersion("1.0.0")]
        public sealed class FirstMigration : Migration
        {
            public override async Task ApplyAsync(MigrationSession context) =>
                await context.Session.StoreAsync(new Entity { Value = "Foo" });
        }

        [MigrationVersion("2.0.0")]
        public sealed class SecondMigration : Migration
        {
            public override async Task ApplyAsync(MigrationSession context) =>
                await context.Session.StoreAsync(new Entity { Value = "Bar" });
        }

        public sealed class Entity
        {
            public string? Id { get; set; }

            public string? Value { get; set; }
        }
    }
}