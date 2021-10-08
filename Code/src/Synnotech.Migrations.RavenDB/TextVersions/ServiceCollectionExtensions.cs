using System;
using System.Reflection;
using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.TextVersions;

namespace Synnotech.Migrations.RavenDB.TextVersions
{
    /// <summary>
    /// Provides methods to register the migration engine with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// <para>
        /// Registers the default migration engine for Raven DB with the DI Container. The migration engine uses text versions.
        /// All instances (<see cref="MigrationEngine" />, <see cref="SessionFactory" />, and <see cref="MicrosoftDependencyInjectionMigrationFactory{TMigration}" />)
        /// will be added with a transient lifetime. A registration for <see cref="IDocumentStore" /> must already be present
        /// (usually with a singleton lifetime).
        /// </para>
        /// <para>
        /// IMPORTANT: you should not call this method when you run a custom setup -
        /// please register your own types with the DI container in this case.
        /// </para>
        /// </summary>
        /// <param name="services">The service collection used to register types with the DI container.</param>
        /// <param name="assembliesContainingMigrations">
        /// The assemblies that will be searched for migration types (optional). If you do not provide any assemblies,
        /// the calling assembly will be searched.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
        public static IServiceCollection AddSynnotechMigrations(this IServiceCollection services, params Assembly[] assembliesContainingMigrations)
        {
            services.MustNotBeNull(nameof(services));

            if (assembliesContainingMigrations.IsNullOrEmpty())
                assembliesContainingMigrations = new[] { Assembly.GetCallingAssembly() };

            return services.AddTransient<ISessionFactory<MigrationInfo, Migration, IAsyncDocumentSession>, SessionFactory>()
                           .AddTransient<IMigrationFactory<Migration>>(container => new MicrosoftDependencyInjectionMigrationFactory<Migration>(container))
                           .AddTransient<Func<Migration, DateTime, MigrationInfo>>(_ => MigrationInfo.Create)
                           .AddTransient<MigrationEngine>()
                           .AddMigrationTypes<Migration, MigrationVersionAttribute>(assembliesContainingMigrations);
        }
    }
}