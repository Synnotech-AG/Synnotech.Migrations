using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.Linq2Db.Int64TimestampVersions;
using Synnotech.MsSqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.Linq2Db.Tests.Int64TimestampVersions
{
    public sealed class MigrationEngineTests
    {
        public MigrationEngineTests(ITestOutputHelper output) => Output = output;
        private ITestOutputHelper Output { get; }

        [SkippableFact]
        public async Task ApplyAllMigrations()
        {
            await using var container = await InitializeOrSkipTestAsync();
            var now = DateTime.UtcNow;
            var migrationEngine = container.GetRequiredService<MigrationEngine>();

            var summary = await migrationEngine.MigrateAsync(now);
            Output.LogSummary(summary);

            await using var dataConnection = container.GetRequiredService<DataConnection>();
            var migrationInfos = await dataConnection.GetTable<MigrationInfo>()
                                                     .ToListAsync();
            var expectedMigrationInfos = new MigrationInfo[]
            {
                new () { Id = 1, Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now },
                new () { Id = 2, Name = nameof(SomeContacts), Version = 20211008111259, AppliedAt = now }
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
                await dataConnection.CreateTableAsync<Contact>();
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = DateTime.UtcNow });
            }

            var now = DateTime.UtcNow;
            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync(now);
            Output.LogSummary(summary);

            var expectedMigrations = new List<MigrationInfo>(1)
            {
                new () { Id = 2, Name = nameof(SomeContacts), Version = 20211008111259, AppliedAt = now }
            };
            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            appliedMigrations.Should().BeEquivalentTo(expectedMigrations);
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
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now });
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(SomeContacts), Version = 20211008111259, AppliedAt = now });
            }

            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync();
            Output.LogSummary(summary);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
            appliedMigrations.Should().BeNull();
            summary.EnsureSuccess();
        }

        [Fact]
        public async Task RunPreviousMigration()
        {
            await using var container = await InitializeOrSkipTestAsync();
            var now = DateTime.UtcNow;
            await using (var dataConnection = container.GetRequiredService<DataConnection>())
            {
                await dataConnection.CreateTableAsync<MigrationInfo>();
                await dataConnection.InsertAsync(new MigrationInfo { Name = nameof(SomeContacts), Version = 20211008111259, AppliedAt = now.AddDays(-2) });
            }

            var migrationEngine = container.GetRequiredService<MigrationEngine>();
            var summary = await migrationEngine.MigrateAsync(now, approach: MigrationApproach.AllNonAppliedMigrations);
            Output.LogSummary(summary);

            summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
            var expectedMigrationInfos = new List<MigrationInfo>
            {
                new () { Id = 1, Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now }
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

        [MigrationVersion("2021-10-08T11:11:55Z")]
        public sealed class InitialTableStructure : EmbeddedScriptMigration
        {
            public InitialTableStructure() : base("InitialScript.sql") { }
        }

        [MigrationVersion("2021-10-08T11:12:59Z")]
        public sealed class SomeContacts : Migration
        {
            public override async Task ApplyAsync(DataConnection dataConnection, CancellationToken cancellationToken = default)
            {
                await dataConnection.InsertAsync(new Contact { Name = "John Doe" }, token: cancellationToken);
                await dataConnection.InsertAsync(new Contact { Name = "Jane Foe" }, token: cancellationToken);
            }
        }
    }
}