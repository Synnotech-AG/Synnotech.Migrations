using System;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a migration factory that uses <see cref="Activator" /> to
    /// call the default constructor of a migration class.
    /// </summary>
    /// <typeparam name="TMigration">The base class used for migrations.</typeparam>
    public sealed class ActivatorMigrationFactory<TMigration> : IMigrationFactory<TMigration>
    {
        /// <summary>
        /// Uses <see cref="Activator" /> to call the default constructor of the specified type.
        /// </summary>
        /// <param name="type">The migration type to be instantiated.</param>
        /// <exception cref="MigrationException">
        /// Thrown when any exception occurs during the creation process or when the instance
        /// cannot be cast to <typeparamref name="TMigration" />.
        /// </exception>
        public TMigration CreateMigration(Type type)
        {
            object? instance;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not instantiate migration type \"{type}\".", exception);
            }

            if (instance is not TMigration migration)
                throw new MigrationException($"Type \"{type}\" cannot be cast to migration type \"{typeof(TMigration)}\".");

            return migration;
        }
    }
}