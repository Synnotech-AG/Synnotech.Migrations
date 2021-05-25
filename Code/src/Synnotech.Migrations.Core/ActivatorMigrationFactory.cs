using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a migration factory that uses <see cref="Activator" /> to
    /// call the default constructor of a migration class.
    /// </summary>
    /// <typeparam name="TMigration">The base class used for migrations.</typeparam>
    public sealed class ActivatorMigrationFactory<TMigration> : BaseMigrationFactory<TMigration>
    {
        /// <summary>
        /// Instantiates the given migration type using <see cref="Activator" />.
        /// </summary>
        protected override object InstantiateType(Type migrationType) => Activator.CreateInstance(migrationType);
    }
}