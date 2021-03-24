using Raven.TestDriver;

namespace Synnotech.Migrations.RavenDB.Tests
{
    public abstract class RavenDbIntegrationTest : RavenTestDriver
    {
        static RavenDbIntegrationTest() => ConfigureServer(TestSettings.RavenDbOptions);
    }
}