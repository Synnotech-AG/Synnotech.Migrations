namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents a factory that creates session instances which communicate with the target system.
    /// </summary>
    /// <typeparam name="TMigrationSession">The type of the session.</typeparam>
    /// <typeparam name="TMigration">The base type for all migrations.</typeparam>
    public interface IAsyncSessionFactory<out TMigrationSession, in TMigration>
    {
        /// <summary>
        /// Creates a new session that will be used to retrieve the latest applied migration info.
        /// </summary>
        TMigrationSession CreateSessionForRetrievingLatestMigrationInfo();

        /// <summary>
        /// Creates a new session that will be used to execute the specified migration.
        /// </summary>
        /// <param name="migration">The migration to be executed.</param>
        TMigrationSession CreateSessionForMigration(TMigration migration);
    }
}