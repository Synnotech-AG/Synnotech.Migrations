using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.Linq2Db.Int64TimestampVersions
{
    /// <summary>
    /// Provides methods to register the migration engine with the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// <para>
        /// Registers the default migration engine for LinqToDB with the DI container. This includes the types <see cref="MigrationEngine" />,
        /// <see cref="SessionFactory" />, the <see cref="MicrosoftDependencyInjectionMigrationFactory{TMigration}" /> and <see cref="MigrationInfo.Create" /> as
        /// a delegate - all of them use transient lifetimes.
        /// Additionally, all instantiatable types that derive from <see cref="Migration" /> will be registered with the DI container. This way you can
        /// use dependency injection directly in your migration classes. The migration engine will dispose your migrations when they implement <see cref="IAsyncDisposable" />
        /// or <see cref="IDisposable" />.
        /// </para>
        /// <para>
        /// The session factory requires a Func&lt;DataConnection&gt; to be already registered with the DI container.
        /// This is used to resolve a data connection when constructing session instances.
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
        [MethodImpl(MethodImplOptions.NoInlining)]
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