using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;
using Synnotech.Migrations.RavenDB.TextVersions;
using Xunit;

namespace Synnotech.Migrations.RavenDB.Tests.TextVersions
{
    public sealed class RavenMigrationEngineTests : RavenDbIntegrationTest
    {
        [SkippableFact]
        public async Task MigrateAll()
        {
            TestSettings.SkipTestIfNecessary();

            var now = DateTime.UtcNow;
            await using var container = CreateContainer();
            var engine = container.GetRequiredService<MigrationEngine>();

            var summary = await engine.MigrateAsync(now);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new () { Id = "migrationInfos/1.0.0", Name = nameof(FirstMigration), Version = "1.0.0", AppliedAt = now },
                new () { Id = "migrationInfos/2.0.0", Name = nameof(SecondMigration), Version = "2.0.0", AppliedAt = now }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
            using var session = container.GetRequiredService<IDocumentStore>().OpenAsyncSession();
            var storedEntities = await session.Query<Entity>().ToListAsync();
            var expectedStoredEntities = new List<Entity>
            {
                new () { Id = "entities/1-A", Value = "Foo" },
                new () { Id = "entities/2-A", Value = "Bar" }
            };
            storedEntities.Should().BeEquivalentTo(expectedStoredEntities, config => config.WithStrictOrdering());
            var storedMigrationInfos = await session.Query<MigrationInfo>().ToListAsync();
            storedMigrationInfos.Should().BeEquivalentTo(expectedMigrationInfos, config => config.WithStrictOrdering());
        }

        [SkippableFact]
        public async Task MigrateNewest()
        {
            TestSettings.SkipTestIfNecessary();

            var now = DateTime.UtcNow;
            await using var container = CreateContainer();
            var engine = container.GetRequiredService<MigrationEngine>();
            using var session = container.GetRequiredService<IDocumentStore>().OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            await session.StoreAsync(new MigrationInfo { Id = "migrationInfos/1.0.0", Name = nameof(FirstMigration), Version = "1.0.0", AppliedAt = now.AddDays(-1) });
            await session.SaveChangesAsync();

            var summary = await engine.MigrateAsync(now, new[] { GetType().Assembly });

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfo = new MigrationInfo { Id = "migrationInfos/2.0.0", Name = nameof(SecondMigration), Version = "2.0.0", AppliedAt = now };
            var expectedMigrationInfos = new List<MigrationInfo> { expectedMigrationInfo };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
            var storedEntities = await session.Query<Entity>().ToListAsync();
            var expectedStoredEntities = new List<Entity> { new () { Id = "entities/1-A", Value = "Bar" } };
            storedEntities.Should().BeEquivalentTo(expectedStoredEntities);
            var storedMigrationInfo = await session.LoadAsync<MigrationInfo?>(expectedMigrationInfo.Id);
            storedMigrationInfo.Should().BeEquivalentTo(expectedMigrationInfo);
        }

        [SkippableFact]
        public async Task AllMigrationsApplied()
        {
            TestSettings.SkipTestIfNecessary();

            await using var container = CreateContainer();
            var engine = container.GetRequiredService<MigrationEngine>();
            using var session = container.GetRequiredService<IDocumentStore>().OpenAsyncSession();
            session.Advanced.WaitForIndexesAfterSaveChanges();
            await session.StoreAsync(new MigrationInfo { Id = "migrationInfos/2.0.0", Name = nameof(FirstMigration), Version = "2.0.0", AppliedAt = DateTime.UtcNow.AddDays(-5) });
            await session.SaveChangesAsync();

            var summary = await engine.MigrateAsync();

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNullOrEmpty();
            var entities = await session.Query<Entity>().ToListAsync();
            entities.Should().BeNullOrEmpty();
        }

        [SkippableFact]
        public async Task GetMigrationPlan()
        {
            TestSettings.SkipTestIfNecessary();

            await using var container = CreateContainer();

            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var migrationPlan = await migrationEngine.GetPlanForNewMigrationsAsync(new[] { typeof(RavenMigrationEngineTests).Assembly });

            var expectedPlan = new MigrationPlan<Version, MigrationInfo>(null, new List<PendingMigration<Version>> { ToPendingMigration(typeof(FirstMigration)), ToPendingMigration(typeof(SecondMigration)) });
            migrationPlan.Should().Be(expectedPlan);
        }

        private ServiceProvider CreateContainer([CallerMemberName] string? databaseName = null) =>
            PrepareContainer(databaseName).AddSynnotechMigrations()
                                          .BuildServiceProvider();

        private static PendingMigration<Version> ToPendingMigration(Type migrationType)
        {
            if (!migrationType.CheckIfTypeIsMigration<MigrationVersionAttribute>(typeof(Migration), out var migrationAttribute))
                throw new ArgumentException($"The type \"{migrationType}\" is not a migration type.", nameof(migrationType));
            return new PendingMigration<Version>(migrationAttribute.Version, migrationType);
        }

        [MigrationVersion("1.0.0")]
        public sealed class FirstMigration : Migration
        {
            public override Task ApplyAsync(IAsyncDocumentSession session, CancellationToken cancellationToken = default) =>
                session.StoreAsync(new Entity { Value = "Foo" }, cancellationToken);
        }

        [MigrationVersion("2.0.0")]
        public sealed class SecondMigration : Migration
        {
            public override Task ApplyAsync(IAsyncDocumentSession session, CancellationToken cancellationToken = default) =>
                session.StoreAsync(new Entity { Value = "Bar" }, cancellationToken);
        }

        public sealed class Entity
        {
            public string? Id { get; set; }

            public string? Value { get; set; }
        }
    }
}