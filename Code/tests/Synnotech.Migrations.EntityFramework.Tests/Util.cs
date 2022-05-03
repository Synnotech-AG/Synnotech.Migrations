using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;
using Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;
using Synnotech.MsSqlServer;

namespace Synnotech.Migrations.EntityFramework.Tests;

public static class Util
{
    public static async Task<ServiceProvider> InitializeOrSkipTestAsync()
    {
        var connectionString = TestSettings.GetConnectionString();
        await Database.DropAndCreateDatabaseAsync(connectionString);

        return new ServiceCollection()
              .AddDatabaseContext(connectionString)
              .AddSynnotechMigrations<DatabaseContext>()
              .BuildServiceProvider();
    }
}