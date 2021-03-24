using Microsoft.Extensions.Configuration;
using Raven.TestDriver;
using Xunit;

namespace Synnotech.Migrations.RavenDB.Tests
{
    public static class TestSettings
    {
        static TestSettings()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("testsettings.json", true)
                                                      .AddJsonFile("testsettings.Development.json", true)
                                                      .Build();
        }

        public static IConfiguration Configuration { get; }

        public static bool RunDatabaseIntegrationTests => Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

        public static TestServerOptions RavenDbOptions => Configuration.GetSection(nameof(RavenDbOptions)).Get<TestServerOptions>();

        public static void SkipDatabaseIntegrationTestIfNecessary()
        {
            Skip.IfNot(RunDatabaseIntegrationTests);
        }
    }
}