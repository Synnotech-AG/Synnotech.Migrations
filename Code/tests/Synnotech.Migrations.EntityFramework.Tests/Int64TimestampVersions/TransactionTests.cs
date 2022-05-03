using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;

public class TransactionTests
{
    public TransactionTests(ITestOutputHelper output) => Output = output;

    private ITestOutputHelper Output { get; }

    [Fact]
    public async Task MigrationWithoutTransaction_ShouldNotThrow()
    {
        await using var container = await Util.InitializeOrSkipTestAsync();
        using (var dbContext = container.GetRequiredService<DatabaseContext>())
        {
            await dbContext.CreateMigrationInfos();
        }

        var migrationEngine = container.GetRequiredService<MigrationEngine<DatabaseContext>>();
        var summary = await migrationEngine.ApplyMigrationsFromTestClass<TransactionTests>();
        Output.LogSummary(summary);

        summary.EnsureSuccess();
    }

    [MigrationVersion("2022-05-02T17:30:00Z")]
    public sealed class MigrationThrowsIfTransactionIsActive : EmbeddedScriptMigration<DatabaseContext>
    {
        public MigrationThrowsIfTransactionIsActive() : base("ThrowOnTransaction.sql")
        {
            IsRequiringTransaction = false;
        }
    }
}