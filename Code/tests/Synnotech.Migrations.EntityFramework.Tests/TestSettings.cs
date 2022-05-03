using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.Extensions.Configuration;
using Xunit;
using SynnotechTestSettings = Synnotech.Xunit.TestSettings;

namespace Synnotech.Migrations.EntityFramework.Tests;

public static class TestSettings
{
    public static bool RunDatabaseIntegrationTests =>
        SynnotechTestSettings.Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

    public static string GetConnectionString()
    {
        Skip.IfNot(RunDatabaseIntegrationTests);
        var connectionString = SynnotechTestSettings.Configuration["connectionString"];
        if (connectionString.IsNullOrWhiteSpace())
            throw new InvalidConfigurationException("You must set the connectionString in testsettings when RunDatabaseIntegrationTests is set to true");

        return connectionString;
    }
}