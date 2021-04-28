using System;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Linq2Db.TextVersions;

namespace Synnotech.Migrations.Linq2Db.Tests.TextVersions
{
    public static class DatabaseContext
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString, SqlServerVersion sqlServerVersion = SqlServerVersion.v2017)
        {
            var sqlServerDataProvider = (SqlServerDataProvider) SqlServerTools.GetDataProvider(sqlServerVersion, SqlServerProvider.MicrosoftDataSqlClient);
            CreateMappings(sqlServerDataProvider.MappingSchema);
            return services.AddSingleton(sqlServerDataProvider)
                           .AddTransient(c => new DataConnection(c.GetRequiredService<SqlServerDataProvider>(), new SqlConnection(connectionString), true))
                           .AddSingleton<Func<DataConnection>>(c => c.GetRequiredService<DataConnection>);
        }

        public static void CreateMappings(MappingSchema schema)
        {
            var builder = schema.GetFluentMappingBuilder();

#nullable disable
            builder.MapMigrationInfo()
                   .Entity<MasterData>()
                   .HasTableName("MasterData")
                   .Property(person => person.Id).IsIdentity().IsPrimaryKey();
#nullable restore
        }
    }
}