using System.Data.Entity;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;

public class EmbeddedScriptTests
{
    public EmbeddedScriptTests(ITestOutputHelper output) => Output = output;

    private ITestOutputHelper Output { get; }

    [Fact]
    public async Task FailedMigration_ShouldBeRolledBack()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            await dbContext.CreateInitialTables();
        }

        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();
        var summary = await migrationEngine.ApplyMigrationsFromTestClass<EmbeddedScriptTests>();
        Output.LogSummary(summary);

        summary.TryGetError(out _).Should().BeTrue();
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            var climbersCount = await dbContext.RockClimbers.CountAsync();
            climbersCount.Should().Be(0);
        }
    }

    [MigrationVersion("2022-05-02T11:50:00Z")]
    public sealed class FailingMigration : EmbeddedScriptsMigration<DatabaseContext>
    {
        public FailingMigration() : base("InsertClimber.sql", "ThrowError.sql") { }
    }
}