namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the different approaches the migration engine can take when determining and applying migrations.
    /// </summary>
    public enum MigrationApproach
    {
        /// <summary>
        /// The migration engine will load the latest migration info from the target system and then
        /// determine all migrations that have a higher version. This is the default approach and should
        /// be the only approach applied to Staging or Production systems.
        /// </summary>
        MigrationsWithNewerVersions,

        /// <summary>
        /// The migration engine will load all migration infos and check which migrations have not been applied
        /// to the target database (this might include migrations with a lower version than your current latest migration).
        /// This approach should only be used in development scenarios, especially to apply migrations that were
        /// created by co-workers on another branch that has been merged. In most cases, we suspect these migrations
        /// to be independent of your latest migrations, but you can use this approach to spot potentially entanglement
        /// between migrations (we recommend to use automated tests / integration tests to catch these issues).
        /// </summary>
        AllNonAppliedMigrations
    }
}