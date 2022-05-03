using System;
using System.Data.Entity;
using System.Reflection;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.Migrations.Core;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// Provides methods to register the migration engine with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// <para>
    /// Registers the default migration engine for EntityFramework with the DI container. This includes the types <see cref="MigrationEngine{TMigration,TMigrationInfo,TMigrationContext}" />,
    /// <see cref="SessionFactory{TDbContext}" />, the <see cref="MicrosoftDependencyInjectionMigrationFactory{TMigration}" /> and <see cref="MigrationInfo.Create{TDbContext}" /> as
    /// a delegate - all of them use transient lifetimes.
    /// Additionally, all instantiatable types that derive from <see cref="Migration{TDbContext}" /> will be registered with the DI container. This way you can
    /// use dependency injection directly in your migration classes. The migration engine will dispose your migrations when they implement <see cref="IAsyncDisposable" />
    /// or <see cref="IDisposable" />.
    /// </para>
    /// <para>
    /// The session factory requires a Func&lt;TDbContext&gt; to be already registered with the DI container.
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
    public static IServiceCollection AddSynnotechMigrations<TDbContext>(this IServiceCollection services,
                                                                        params Assembly[] assembliesContainingMigrations)
        where TDbContext : DbContext, IHasMigrationInfos<MigrationInfo>
    {
        services.MustNotBeNull(nameof(services));

        if (assembliesContainingMigrations.IsNullOrEmpty())
            assembliesContainingMigrations = new[] { Assembly.GetCallingAssembly() };

        return services.AddTransient<ISessionFactory<MigrationInfo, Migration<TDbContext>, TDbContext>, SessionFactory<TDbContext>>()
                       .AddTransient<IMigrationFactory<Migration<TDbContext>>>(container => new MicrosoftDependencyInjectionMigrationFactory<Migration<TDbContext>>(container))
                       .AddTransient<Func<Migration<TDbContext>, DateTime, MigrationInfo>>(_ => MigrationInfo.Create)
                       .AddTransient<MigrationEngine<TDbContext>>()
                       .AddMigrationTypes<Migration<TDbContext>, MigrationVersionAttribute>(assembliesContainingMigrations);
    }
}