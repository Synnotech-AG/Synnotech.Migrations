using System;
using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a migration factory that uses the <see cref="IServiceProvider" /> DI container
    /// to instantiate migrations.
    /// </summary>
    /// <typeparam name="TMigration">The base class used for migrations.</typeparam>
    public sealed class MicrosoftDependencyInjectionMigrationFactory<TMigration> : BaseMigrationFactory<TMigration>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MicrosoftDependencyInjectionMigrationFactory{TMigration}" />.
        /// </summary>
        /// <param name="serviceProvider">The service provider that is used to resolve migration types.</param>
        public MicrosoftDependencyInjectionMigrationFactory(IServiceProvider serviceProvider) =>
            ServiceProvider = serviceProvider.MustNotBeNull(nameof(serviceProvider));

        private IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Instantiates the given migration type using the <see cref="IServiceProvider" /> container.
        /// </summary>
        protected override object InstantiateType(Type migrationType) =>
            ServiceProvider.GetRequiredService(migrationType);
    }
}