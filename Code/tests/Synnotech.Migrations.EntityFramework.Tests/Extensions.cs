using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Light.EmbeddedResources;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;
using Xunit.Abstractions;

namespace Synnotech.Migrations.EntityFramework.Tests;

public static class Extensions
{
    public static void LogSummary<T>(this ITestOutputHelper output, MigrationSummary<T> summary)
    {
        if (summary.TryGetAppliedMigrations(out var appliedMigrations))
        {
            foreach (var appliedMigration in appliedMigrations)
            {
                output.WriteLine(appliedMigration!.ToString());
            }
        }

        output.WriteLine(string.Empty);

        if (summary.TryGetError(out var error))
            output.WriteLine(error.Exception.ToString());
    }

    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString) =>
        services.AddTransient(_ => new DatabaseContext(connectionString))
                .AddSingleton<Func<DatabaseContext>>(c => c.GetRequiredService<DatabaseContext>);

    public static async Task<MigrationSummary<MigrationInfo>> ApplyMigrationsFromTestClass<T>(
        this MigrationEngine<DatabaseContext> migrationEngine,
        MigrationApproach approach = MigrationApproach.MigrationsWithNewerVersions)
    {
        var testMigrations = new List<PendingMigration<long>>();

        foreach (var nestedType in typeof(T).GetNestedTypes())
        {
            var versionAttribute = nestedType.GetCustomAttribute<MigrationVersionAttribute>();
            if (nestedType.IsClass && versionAttribute is not null)
            {
                var migration = new PendingMigration<long>(versionAttribute.Version, nestedType);
                testMigrations.Add(migration);
            }
        }

        var migrationPlan = approach == MigrationApproach.MigrationsWithNewerVersions ?
                                await migrationEngine.GetPlanForNewMigrationsAsync() :
                                await migrationEngine.GetPlanForNonAppliedMigrationsAsync();

        var pendingMigrations = migrationPlan.PendingMigrations!
                                             .Intersect(testMigrations)
                                             .ToList();

        return await migrationEngine.ApplyMigrationsAsync(pendingMigrations);
    }


    public static async Task CreateInitialTables(this DatabaseContext dbContext)
    {
        await dbContext.CreateMigrationInfos();
        await dbContext.CreateRockClimbers();
    }

    public static Task CreateRockClimbers(this DatabaseContext dbContext) =>
        dbContext.Database.ExecuteSqlCommandAsync(typeof(DatabaseContext).GetEmbeddedResource("CreateRockClimbers.sql"));

    public static Task CreateMigrationInfos(this DatabaseContext dbContext) =>
        dbContext.Database.ExecuteSqlCommandAsync(typeof(DatabaseContext).GetEmbeddedResource("CreateMigrationInfos.sql"));
}