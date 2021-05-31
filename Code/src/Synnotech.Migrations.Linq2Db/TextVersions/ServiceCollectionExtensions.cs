using System;
using System.Reflection;
using Light.GuardClauses;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Provides methods to register the migration engine with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default migration engine for Linq2Db with the DI container.
        /// This method registers <see cref="MigrationEngine" /> and <see cref="SessionFactory" />
        /// with transient lifetimes.
        /// The session factory requires a Func&lt;DataConnection&gt; to be already registered with the DI container.
        /// This is used to resolve a data connection when constructing session instances.
        /// IMPORTANT: you should not call this method when you run a custom setup -
        /// please register your own types with the DI container in this case.
        /// </summary>
        /// <param name="services">The service collection used to register types with the DI container.</param>
        /// <param name="assembliesContainingMigrations">
        /// The assemblies that will be searched for migration types (optional). If you do not provide any assemblies,
        /// the calling assembly will be searched.
        /// </param>
        public static IServiceCollection AddSynnotechMigrations(this IServiceCollection services,
                                                                params Assembly[] assembliesContainingMigrations)
        {
            services.MustNotBeNull(nameof(services));

            if (assembliesContainingMigrations.IsNullOrEmpty())
                assembliesContainingMigrations = new[] { Assembly.GetCallingAssembly() };

            return services.AddTransient<ISessionFactory<MigrationInfo, Migration, DataConnection>, SessionFactory>()
                           .AddTransient<IMigrationFactory<Migration>>(container => new MicrosoftDependencyInjectionMigrationFactory<Migration>(container))
                           .AddTransient<Func<Migration, DateTime, MigrationInfo>>(_ => MigrationInfo.Create)
                           .AddTransient<MigrationEngine>()
                           .AddMigrationTypes<Migration, MigrationVersionAttribute>(assembliesContainingMigrations);
        }
    }
}