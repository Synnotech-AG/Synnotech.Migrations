using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;

public sealed class MigrationEngineTests
{
    public MigrationEngineTests(ITestOutputHelper output) => Output = output;
    private ITestOutputHelper Output { get; }

    [SkippableFact]
    public async Task ApplyAllMigrations()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        var now = DateTime.UtcNow;
        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();

        var summary = await migrationEngine.ApplyMigrationsFromTestClass<MigrationEngineTests>();
        Output.LogSummary(summary);

        using var dbContext = container.GetRequiredService<DatabaseContext>();
        var migrationInfos = await dbContext.MigrationInfos.ToListAsync();
        var expectedMigrationInfos = new MigrationInfo[]
        {
            new() { Id = 1, Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now },
            new() { Id = 2, Name = nameof(SomeClimbers), Version = 20211008111259, AppliedAt = now }
        };
        migrationInfos.Should().BeEquivalentTo(expectedMigrationInfos, options => options.WithStrictOrdering());
    }

    [SkippableFact]
    public async Task ApplyNewestMigration()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            await dbContext.CreateInitialTables();
            dbContext.MigrationInfos.Add(new MigrationInfo { Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
        }

        var now = DateTime.UtcNow;
        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();
        var summary = await migrationEngine.ApplyMigrationsFromTestClass<MigrationEngineTests>();
        Output.LogSummary(summary);
        var expectedMigrations = new List<MigrationInfo>(1)
        {
            new() { Id = 2, Name = nameof(SomeClimbers), Version = 20211008111259, AppliedAt = now }
        };
        summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
        appliedMigrations.Should().BeEquivalentTo(expectedMigrations);
        summary.EnsureSuccess();
    }

    [SkippableFact]
    public async Task NoMigrationsAvailable()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        var now = DateTime.UtcNow;
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            await dbContext.CreateInitialTables();
            dbContext.MigrationInfos.AddRange(new[]
            {
                new MigrationInfo { Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now },
                new MigrationInfo { Name = nameof(SomeClimbers), Version = 20211008111259, AppliedAt = now },
            });
            await dbContext.SaveChangesAsync();
        }

        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();
        var summary = await migrationEngine.ApplyMigrationsFromTestClass<MigrationEngineTests>();
        Output.LogSummary(summary);
        summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeFalse();
        appliedMigrations.Should().BeNull();
        summary.EnsureSuccess();
    }

    [Fact]
    public async Task RunPreviousMigration()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        var now = DateTime.UtcNow;
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            await dbContext.CreateMigrationInfos();
            dbContext.MigrationInfos.Add(new MigrationInfo { Name = nameof(SomeClimbers), Version = 20211008111259, AppliedAt = now.AddDays(-2) });
            await dbContext.SaveChangesAsync();
        }

        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();
        var summary = await migrationEngine.ApplyMigrationsFromTestClass<MigrationEngineTests>(MigrationApproach.AllNonAppliedMigrations);
        Output.LogSummary(summary);

        summary.TryGetAppliedMigrations(out var appliedMigrations).Should().BeTrue();
        var expectedMigrationInfos = new List<MigrationInfo>
        {
            new() { Id = 1, Name = nameof(InitialTableStructure), Version = 20211008111155, AppliedAt = now }
        };
        appliedMigrations.Should().BeEquivalentTo(expectedMigrationInfos);
    }

    [MigrationVersion("2021-10-08T11:11:55Z")]
    public sealed class InitialTableStructure : EmbeddedScriptsMigration<DatabaseContext>
    {
        public InitialTableStructure() : base("CreateMigrationInfos.sql", "CreateRockClimbers.sql") { }
    }

    [MigrationVersion("2021-10-08T11:12:59Z")]
    public sealed class SomeClimbers : Migration<DatabaseContext>
    {
        public override Task ApplyAsync(DatabaseContext dbContext, CancellationToken cancellationToken = default)
        {
            dbContext.RockClimbers.AddRange(new[]
            {
                new RockClimber { Name = "Lynn Hill" },
                new RockClimber { Name = "John Bachar" },
            });

            return Task.CompletedTask;
        }
    }
}