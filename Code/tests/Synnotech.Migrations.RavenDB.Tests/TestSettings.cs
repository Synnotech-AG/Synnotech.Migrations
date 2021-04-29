using Microsoft.Extensions.Configuration;
using Raven.TestDriver;
using Xunit;
using SynnotechTestSettings = Synnotech.Xunit.TestSettings;

namespace Synnotech.Migrations.RavenDB.Tests
{
    public static class TestSettings
    {
        public static bool RunDatabaseIntegrationTests =>
            SynnotechTestSettings.Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

        public static TestServerOptions RavenDbOptions =>
            SynnotechTestSettings.Configuration.GetSection(nameof(RavenDbOptions)).Get<TestServerOptions>();

        public static void SkipTestIfNecessary() => Skip.IfNot(RunDatabaseIntegrationTests);
    }
}