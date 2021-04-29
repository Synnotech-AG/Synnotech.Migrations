using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.Extensions.Configuration;
using Xunit;

// ReSharper disable once CheckNamespace -- this class should be partial with the imported class
namespace Synnotech
{
    public static partial class TestSettings
    {
        public static bool RunDatabaseIntegrationTests => Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

        public static void SkipIntegrationTestIfNecessary() => Skip.IfNot(RunDatabaseIntegrationTests);

        public static string GetConnectionString()
        {
            SkipIntegrationTestIfNecessary();
            var connectionString = Configuration["connectionString"];
            if (connectionString.IsNullOrWhiteSpace())
                throw new InvalidConfigurationException("You must configure \"connectionString\" in testsettings.json when \"runDatabaseIntegrationTests\" is set to true");
            return connectionString;
        }
    }
}