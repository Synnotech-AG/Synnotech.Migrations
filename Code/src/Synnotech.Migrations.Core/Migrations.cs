using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Provides extension methods for migrations.
    /// </summary>
    public static class Migrations
    {
        /// <summary>
        /// Registers all migrations in the specified assemblies with the DI container using a transient lifetime.
        /// A type is considered a migration when it is a public non-abstract class deriving from <typeparamref name="TMigration" />,
        /// and when it has a valid migration attribute, represented by <typeparamref name="TMigrationAttribute" />.
        /// </summary>
        /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
        /// <typeparam name="TMigrationAttribute">The type that represents the attribute being applied to migrations to indicate their version.</typeparam>
        /// <param name="services">The service collection used to register types with the DI container.</param>
        /// <param name="assembliesContainingMigrations">The assemblies that will be searched for migration types.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assembliesContainingMigrations" /> is an empty array.</exception>
        /// <exception cref="MigrationException">Thrown when any migration type is found whose migration attribute is invalid.</exception>
        public static IServiceCollection AddMigrationTypes<TMigration, TMigrationAttribute>(this IServiceCollection services,
                                                                                            params Assembly[] assembliesContainingMigrations)
            where TMigrationAttribute : Attribute, IMigrationAttribute
        {
            services.MustNotBeNull(nameof(services));
            assembliesContainingMigrations.MustNotBeNullOrEmpty(nameof(assembliesContainingMigrations));

            RegisterMigrationTypes<TMigration, TMigrationAttribute>(type => services.AddTransient(type), assembliesContainingMigrations);

            return services;
        }

        /// <summary>
        /// Registers all migrations in the specified assemblies with the DI container.
        /// A type is considered a migration when it is a public non-abstract class deriving from <typeparamref name="TMigration" />,
        /// and when it has a valid migration attribute, represented by <typeparamref name="TMigrationAttribute" />.
        /// </summary>
        /// <typeparam name="TMigration">The base class that identifies all migrations.</typeparam>
        /// <typeparam name="TMigrationAttribute">The type that represents the attribute being applied to migrations to indicate their version.</typeparam>
        /// <param name="registerMigrationType">The delegate that performs the registration with the DI container. We suggest to perform a transient registration.</param>
        /// <param name="assembliesContainingMigrations">The assemblies that will be searched for migration types.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assembliesContainingMigrations" /> is an empty array.</exception>
        /// <exception cref="MigrationException">Thrown when any migration type is found whose migration attribute is invalid.</exception>
        public static void RegisterMigrationTypes<TMigration, TMigrationAttribute>(Action<Type> registerMigrationType,
                                                                                   params Assembly[] assembliesContainingMigrations)
            where TMigrationAttribute : Attribute, IMigrationAttribute
        {
            registerMigrationType.MustNotBeNull(nameof(registerMigrationType));
            assembliesContainingMigrations.MustNotBeNullOrEmpty(nameof(assembliesContainingMigrations));

            var migrationBaseType = typeof(TMigration);
            foreach (var assembly in assembliesContainingMigrations)
            {
                FindMigrationTypesAndRegisterThem(registerMigrationType, assembly, migrationBaseType);
            }

            static void FindMigrationTypesAndRegisterThem(Action<Type> registerMigrationType, Assembly assembly, Type migrationBaseType)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.CheckIfTypeIsMigration<TMigrationAttribute>(migrationBaseType, out _))
                        registerMigrationType(type);
                }
            }
        }

        /// <summary>
        /// Checks if the specified type is a migration. This is true when the type is a class,
        /// is not abstract, and derives from the specified <paramref name="migrationBaseType" />.
        /// Additionally, the migration attribute must be applied to the type and be valid.
        /// </summary>
        /// <typeparam name="TMigrationAttribute">The type that represents the attribute being applied to migrations to indicate their version.</typeparam>
        /// <param name="type">The type to be checked.</param>
        /// <param name="migrationBaseType">The base type for all migrations.</param>
        /// <param name="attribute">When every check passed, the attribute that is applied to the migration type.</param>
        /// <returns>True if the specified <paramref name="type" /> is a migration type, else false.</returns>
        /// <exception cref="MigrationException">Thrown when <paramref name="type" /> has an invalid migration attribute applied to it.</exception>
        public static bool CheckIfTypeIsMigration<TMigrationAttribute>(this Type type,
                                                                       Type migrationBaseType,
                                                                       [NotNullWhen(true)] out TMigrationAttribute? attribute)
            where TMigrationAttribute : Attribute, IMigrationAttribute
        {
            type.MustNotBeNull(nameof(type));
            migrationBaseType.MustNotBeNull(nameof(migrationBaseType));

            if (!type.IsClass || type.IsAbstract || !type.DerivesFrom(migrationBaseType))
            {
                attribute = null;
                return false;
            }

            attribute = type.GetCustomAttribute<TMigrationAttribute>();
            if (attribute == null)
                return false;

            attribute.Validate(type);
            return true;
        }
    }
}