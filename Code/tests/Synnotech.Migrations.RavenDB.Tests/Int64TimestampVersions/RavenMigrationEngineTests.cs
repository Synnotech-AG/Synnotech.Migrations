using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.RavenDB.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.RavenDB.Tests.Int64TimestampVersions
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
            var firstTimestamp = TimestampParser.ParseTimestamp("2021-10-04T12:06:37Z");
            var secondTimestamp = TimestampParser.ParseTimestamp("2021-10-04T12:12:45Z");
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new () { Id = "migrationInfos/" + firstTimestamp, Name = nameof(FirstMigration), Version = firstTimestamp, AppliedAt = now },
                new () { Id = "migrationInfos/" + secondTimestamp, Name = nameof(SecondMigration), Version = secondTimestamp, AppliedAt = now }
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

        private ServiceProvider CreateContainer([CallerMemberName] string? databaseName = null) => PrepareContainer(databaseName).AddSynnotechMigrations().BuildServiceProvider();
    }

    [MigrationVersion("2021-10-04T12:06:37Z")]
    public sealed class FirstMigration : Migration
    {
        public override Task ApplyAsync(IAsyncDocumentSession session, CancellationToken cancellationToken = default) =>
            session.StoreAsync(new Entity { Value = "Foo" }, cancellationToken);
    }

    [MigrationVersion("2021-10-04T12:12:45Z")]
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