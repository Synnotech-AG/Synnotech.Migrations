using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.RavenDB.TextVersions;

namespace Synnotech.Migrations.RavenDB
{
    /// <summary>
    /// Provides methods to register the migration engine with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default migration engine for Raven DB with the DI Container.
        /// If there is no registration for <see cref="IAsyncDocumentSession"/>, it will
        /// be added with a transient lifetime. All other instances (<see cref="MigrationEngine"/> and <see cref="SessionFactory"/>)
        /// will be also be added with a transient lifetime. A registration for <see cref="IDocumentStore"/> must already be present
        /// (usually with a singleton lifetime).
        /// IMPORTANT: you should not call this method when you run a custom setup -
        /// please register your own types with the DI container in this case.
        /// </summary>
        public static IServiceCollection AddSynnotechMigrations(this IServiceCollection services)
        {
            services.MustNotBeNull(nameof(services));

            services.TryAddTransient(container => container.GetRequiredService<IDocumentStore>().OpenAsyncSession());
            return services.AddTransient<SessionFactory>()
                    .AddTransient(container => new MigrationEngine(container.GetRequiredService<SessionFactory>()));
        }
    }
}