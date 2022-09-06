using System;
using System.Threading.Tasks;
using FluentAssertions;
using Light.EmbeddedResources;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Synnotech.Migrations.Linq2Db.TextVersions;
using Synnotech.MsSqlServer;
using Xunit;

namespace Synnotech.Migrations.Linq2Db.Tests.TextVersions;

public static class LinqToDbGetLatestMigrationInfoSessionTests
{
    [SkippableFact]
    public static async Task GetLatestMigrations()
    {
        var (connectionString, sqlServerVersion) = TestSettings.GetConnectionSettingsOrSkip();

        await Database.DropAndCreateDatabaseAsync(connectionString);
        await Database.ExecuteNonQueryAsync(connectionString, GetInitialTableStructureScript());
        var dataConnection = CreateDataConnection(connectionString, sqlServerVersion);
        await dataConnection.InsertALotOfMigrationInfosAsync();
        var session = new LinqToDbGetLatestMigrationInfoSession(dataConnection);
        var latestMigration = await session.GetLatestMigrationInfoAsync();
        latestMigration!.Version.Should().Be("115.0.0");
    }

    private static DataConnection CreateDataConnection(string connectionString, SqlServerVersion sqlServerVersion)
    {
        var sqlServerDataProvider = SqlServerTools.GetDataProvider(sqlServerVersion);
        var options = new LinqToDBConnectionOptionsBuilder().UseConnectionString(sqlServerDataProvider, connectionString)
                                                            .UseMappingSchema(DatabaseContext.CreateMappings())
                                                            .Build();
        return new DataConnection(options);
    }
    private static async Task InsertALotOfMigrationInfosAsync(this DataConnection dataConnection)
    {
        for (var i = 0; i < 115; i++)
        {
            var migrationInfo = new MigrationInfo { AppliedAt = DateTime.UtcNow.AddDays(-(115 - i)), Name = "Migration" + (i+1), Version = new Version(i + 1, 0, 0).ToString(3) };
            await dataConnection.InsertAsync(migrationInfo);
        }
    }

    private static string GetInitialTableStructureScript() => typeof(LinqToDbGetLatestMigrationInfoSessionTests).GetEmbeddedResource("0.1.0 Initial Table Structure.sql");
}