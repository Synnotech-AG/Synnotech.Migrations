using System;
using LinqToDB.Configuration;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Linq2Db.TextVersions;

namespace Synnotech.Migrations.Linq2Db.Tests.TextVersions
{
    public static class DatabaseContext
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, string connectionString, SqlServerVersion sqlServerVersion = SqlServerVersion.v2017)
        {
            var sqlServerDataProvider = SqlServerTools.GetDataProvider(sqlServerVersion);
            return services.AddSingleton(sqlServerDataProvider)
                           .AddSingleton(c => new LinqToDBConnectionOptionsBuilder().UseConnectionString(c.GetRequiredService<IDataProvider>(), connectionString)
                                                                                    .UseMappingSchema(CreateMappings())
                                                                                    .Build())
                           .AddTransient(c => new DataConnection(c.GetRequiredService<LinqToDBConnectionOptions>()))
                           .AddSingleton<Func<DataConnection>>(c => c.GetRequiredService<DataConnection>);
        }

        public static MappingSchema CreateMappings()
        {
            var schema = new MappingSchema();
            var builder = schema.GetFluentMappingBuilder();

            builder.MapMigrationInfo()
                   .Entity<MasterData>()
                   .HasTableName("MasterData")
                   .Property(person => person.Id).IsIdentity().IsPrimaryKey();

            return schema;
        }
    }
}