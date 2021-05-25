using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.TestDriver;

namespace Synnotech.Migrations.RavenDB.Tests
{
    public abstract class RavenDbIntegrationTest : RavenTestDriver
    {
        static RavenDbIntegrationTest() => ConfigureServer(TestSettings.RavenDbOptions);

        protected IServiceCollection PrepareContainer([CallerMemberName] string? databaseName = null) =>
            new ServiceCollection().AddSingleton(GetDocumentStore(null, databaseName));
    }
}