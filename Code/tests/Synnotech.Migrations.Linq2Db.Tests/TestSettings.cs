using System;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Configuration;
using Xunit;
using SynnotechTestSettings = Synnotech.Xunit.TestSettings;

namespace Synnotech.Migrations.Linq2Db.Tests
{
    public static class TestSettings
    {
        public static bool RunDatabaseIntegrationTests =>
            SynnotechTestSettings.Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

        public static (string connectionString, SqlServerVersion sqlServerVersion) GetConnectionSettingsOrSkip()
        {
            Skip.IfNot(RunDatabaseIntegrationTests);
            var connectionString = SynnotechTestSettings.Configuration["connectionString"];
            if (connectionString.IsNullOrWhiteSpace())
                throw new InvalidConfigurationException("You must set the connectionString in testsettings when RunDatabaseIntegrationTests is set to true");

            var sqlServerVersion = SqlServerVersion.v2017;
            var configurationVersion = SynnotechTestSettings.Configuration["sqlServerVersion"];
            if (!configurationVersion.IsNullOrWhiteSpace() && !Enum.TryParse(configurationVersion, out sqlServerVersion))
                throw new InvalidConfigurationException("sqlServerVersion is set to an invalid value in testsettings.json. Please see LinqToDB.DataProvider.SqlServer.SqlServerVersion enum for valid values.");

            return (connectionString, sqlServerVersion);
        }
    }
}