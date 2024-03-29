﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;
using Synnotech.Migrations.Linq2Db.TextVersions;
using Synnotech.MsSqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.Linq2Db.Tests.TextVersions
{
    public sealed class Linq2DbMigrationEngineTests
    {
        public Linq2DbMigrationEngineTests(ITestOutputHelper output) =>
            Output = output;

        private ITestOutputHelper Output { get; }


        [SkippableFact]
        public async Task ApplyAllMigrations()
        {
            await using var container = await InitializeOrSkipTestAsync();
            var migrationEngine = container.GetRequiredService<MigrationEngine>();

            var now = DateTime.UtcNow;
            var summary = await migrationEngine.MigrateAsync(now);
            Output.LogSummary(summary);

            await using var dataConnection = container.GetRequiredService<DataConnection>();
            var migrationInfos = await dataConnection.GetTable<MigrationInfo>()
                                                     .ToListAsync();
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new () { Id = 1, Name = nameof(InitialTableStructure), Version = "0.1.0", AppliedAt = now },
                new () { Id = 2, Name = nameof(AddFirstMasterDataEntry), Version = "0.2.0", AppliedAt = now }
            };
            migrationInfos.Should().BeEquivalentTo(expectedMigrationInfos, options => options.WithStrictOrdering());
        }

        [SkippableFact]
        public async Task ApplyNewestMigration()
        {
            await using var container = await InitializeOrSkipTestAsync();
            await using (var dataConnection = container.GetRequiredService<DataConnection>())
            {
                await dataConnection.CreateTableAsync<MigrationInfo>();
                await dataConnection.CreateTableAsync<MasterData>();
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(InitialTableStructure), Version = "0.1.0", AppliedAt = DateTime.UtcNow });
            }

            var now = DateTime.UtcNow;
            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync(now);
            Output.LogSummary(summary);

            var expectedMigration = new List<MigrationInfo>
            {
                new () { Id = 2, Name = nameof(AddFirstMasterDataEntry), Version = "0.2.0", AppliedAt = now }
            };
            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            appliedMigrations.Should().BeEquivalentTo(expectedMigration);
            summary.EnsureSuccess();
        }

        [SkippableFact]
        public async Task NoMigrationsAvailable()
        {
            await using var container = await InitializeOrSkipTestAsync();
            var now = DateTime.UtcNow;
            await using (var dataConnection = container.GetRequiredService<DataConnection>())
            {
                await dataConnection.CreateTableAsync<MigrationInfo>();
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(InitialTableStructure), Version = "0.1.0", AppliedAt = now });
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(AddFirstMasterDataEntry), Version = "0.2.0", AppliedAt = now });
            }

            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync(now);
            Output.LogSummary(summary);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNull();
            summary.EnsureSuccess();
        }

        [SkippableFact]
        public async Task RunPreviousMigrations()
        {
            await using var container = await InitializeOrSkipTestAsync();
            var now = DateTime.UtcNow;
            await using (var dataConnection = container.GetRequiredService<DataConnection>())
            {
                await dataConnection.CreateTableAsync<MigrationInfo>();
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(AddFirstMasterDataEntry), Version = "0.2.0", AppliedAt = now.AddDays(-2) });
            }

            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync(now, approach: MigrationApproach.AllNonAppliedMigrations);
            Output.LogSummary(summary);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new () { Id = 2, Name = nameof(InitialTableStructure), Version = "0.1.0", AppliedAt = now }
            };
            appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
        }

        private static async Task<ServiceProvider> InitializeOrSkipTestAsync()
        {
            var (connectionString, sqlServerVersion) = TestSettings.GetConnectionSettingsOrSkip();
            await Database.DropAndCreateDatabaseAsync(connectionString);
            return new ServiceCollection().AddDatabaseContext(connectionString, sqlServerVersion)
                                          .AddSynnotechMigrations()
                                          .BuildServiceProvider();
        }
    }

    [MigrationVersion("0.1.0")]
    public sealed class InitialTableStructure : EmbeddedScriptMigration
    {
        public InitialTableStructure() : base("0.1.0 Initial Table Structure.sql") { }
    }

    [MigrationVersion("0.2.0")]
    public sealed class AddFirstMasterDataEntry : Migration
    {
        public override Task ApplyAsync(DataConnection dataConnection, CancellationToken cancellationToken = default) =>
            dataConnection.InsertAsync(new MasterData(), token: cancellationToken);
    }
}