using Microsoft.Extensions.Configuration;
using Raven.TestDriver;
using Xunit;

// ReSharper disable once CheckNamespace -- this class should be partial with the imported class
namespace Synnotech.Migrations
{
    public static partial class TestSettings
    {
        public static bool RunDatabaseIntegrationTests => Configuration.GetValue<bool>(nameof(RunDatabaseIntegrationTests));

        public static TestServerOptions RavenDbOptions => Configuration.GetSection(nameof(RavenDbOptions)).Get<TestServerOptions>();

        public static void SkipDatabaseIntegrationTestIfNecessary() => Skip.IfNot(RunDatabaseIntegrationTests);
    }
}