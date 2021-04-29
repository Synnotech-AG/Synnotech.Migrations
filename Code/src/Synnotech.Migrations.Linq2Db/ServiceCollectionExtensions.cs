using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Linq2Db.TextVersions;

namespace Synnotech.Migrations.Linq2Db
{
    /// <summary>
    /// Provides methods to register the migration engine with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default migration engine for Linq2Db with the DI container.
        /// This method registers <see cref="MigrationEngine"/> and <see cref="SessionFactory"/>
        /// with transient lifetimes.
        /// The session factory requires a Func&lt;DataConnection&gt; to be already registered with the DI container.
        /// This is used to resolve a data connection when constructing <see cref="MigrationSession"/> instances.
        /// IMPORTANT: you should not call this method when you run a custom setup -
        /// please register your own types with the DI container in this case.
        /// </summary>
        public static IServiceCollection AddSynnotechMigrations(this IServiceCollection services)
        {
            services.MustNotBeNull(nameof(services));

            return services.AddTransient<SessionFactory>()
                           .AddTransient(container => new MigrationEngine(container.GetRequiredService<SessionFactory>()));
        }
    }
}