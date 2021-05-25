using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the base class for migration factories. Subclasses can override how
    /// migration types are instantiated.
    /// </summary>
    /// <typeparam name="TMigration"></typeparam>
    public abstract class BaseMigrationFactory<TMigration> : IMigrationFactory<TMigration>
    {
        /// <summary>
        /// Creates a new migration instance from the specified type.
        /// </summary>
        /// <param name="type">The type of the migration that should be instantiated.</param>
        /// <exception cref="MigrationException">
        /// Thrown when the type could not be instantiated or when the instantiated migration
        /// cannot be cast to type <typeparamref name="TMigration" />.
        /// </exception>
        public TMigration CreateMigration(Type type)
        {
            object? instance;
            try
            {
                instance = InstantiateType(type);
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not instantiate migration type \"{type}\".", exception);
            }

            if (instance is not TMigration migration)
                throw new MigrationException($"Type \"{type}\" cannot be cast to migration type \"{typeof(TMigration)}\".");

            return migration;
        }

        /// <summary>
        /// Instantiates the given type. This method will be called in a try-block, so
        /// exceptions can be thrown.
        /// </summary>
        /// <param name="migrationType">The type of the migration.</param>
        protected abstract object InstantiateType(Type migrationType);
    }
}