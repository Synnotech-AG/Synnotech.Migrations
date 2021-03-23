using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the most generic version of an asynchronous migration engine. It searches for migrations in
    /// the target assembly, compares them to the latest applied migration of the target system
    /// and then applies the pending migrations.
    /// </summary>
    /// <typeparam name="TSession">
    /// The type that represents the session being provided to the migrations. This type must also implement
    /// <see cref="IAsyncMigrationSession{TMigrationInfo}" /> so that the migration engine can
    /// properly execute the necessary queries and commands against the target system.
    /// </typeparam>
    /// <typeparam name="TMigration">The type that represents the general abstraction for migrations.</typeparam>
    /// <typeparam name="TMigrationInfo">
    /// The type that is stored in the target system to identify which migrations have already
    /// been applied.
    /// </typeparam>
    public class AsyncMigrationEngine<TSession, TMigration, TMigrationInfo>
        where TSession : IAsyncMigrationSession<TMigrationInfo>
        where TMigration : class, IAsyncMigration<TSession>, IEquatable<TMigration>, IComparable<TMigration>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AsyncMigrationEngine{TContext,TMigration,TMigrationInfo}" />.
        /// </summary>
        /// <param name="sessionFactory">
        /// The factory that creates sessions to the target system. The session type must implement
        /// <see cref="IAsyncMigrationSession{TMigrationInfo}" />
        /// so that the migration engine can properly execute the necessary queries and commands against the target system.
        /// </param>
        /// <param name="migrationsProvider">The object that retrieves the migrations that need to be applied.</param>
        /// <param name="createMigrationInfo">
        /// The factory delegate that creates migration info objects being stored in
        /// the target system to identify which migrations have already been applied.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public AsyncMigrationEngine(IAsyncSessionFactory<TSession, TMigration> sessionFactory,
                                    IMigrationsProvider<TMigration, TMigrationInfo> migrationsProvider,
                                    Func<TMigration, DateTime, TMigrationInfo> createMigrationInfo)
        {
            SessionFactory = sessionFactory.MustNotBeNull(nameof(sessionFactory));
            MigrationsProvider = migrationsProvider.MustNotBeNull(nameof(migrationsProvider));
            CreateMigrationInfo = createMigrationInfo.MustNotBeNull(nameof(createMigrationInfo));
        }

        /// <summary>
        /// Gets the factory that creates the sessions used to query and update the target system.
        /// </summary>
        public IAsyncSessionFactory<TSession, TMigration> SessionFactory { get; }

        /// <summary>
        /// Gets the object that retrieves the migrations to be applied to the target system.
        /// </summary>
        public IMigrationsProvider<TMigration, TMigrationInfo> MigrationsProvider { get; }

        /// <summary>
        /// Gets the factory delegate that creates a new migration info object.
        /// </summary>
        public Func<TMigration, DateTime, TMigrationInfo> CreateMigrationInfo { get; }

        /// <summary>
        /// Generates a plan that contains information about the latest applied migration on the target system
        /// and the migrations that need to be applied.
        /// </summary>
        /// <param name="migrationAssembly">The target assembly that is to be searched for migrations.</param>
        /// <returns>A plan that describes the latest applied migration and the pending migrations.</returns>
        public virtual async Task<MigrationPlan<TMigration, TMigrationInfo>> GenerateMigrationPlanAsync(Assembly migrationAssembly)
        {
            migrationAssembly.MustNotBeNull(nameof(migrationAssembly));

            using var session = SessionFactory.CreateSessionForRetrievingLatestMigrationInfo();
            var latestMigrationInfo = await session.GetLatestMigrationInfoAsync();
            var migrationsToBeApplied = MigrationsProvider.DetermineMigrations(migrationAssembly, latestMigrationInfo);
            return new MigrationPlan<TMigration, TMigrationInfo>(latestMigrationInfo, migrationsToBeApplied);
        }

        /// <summary>
        /// Determines and applies migrations to the target system. This is done by getting the
        /// latest applied migration info from the target system, picking the migrations
        /// that must be executed, and applying them to the target system.
        /// </summary>
        /// <param name="migrationAssembly">The target assembly that is to be searched for migrations.</param>
        /// <param name="now">
        /// The current time when the migration engine starts to execute. Please use a UTC time stamp if
        /// possible.
        /// </param>
        /// <returns>A summary of all migrations that have been applied in this run.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="migrationAssembly" /> is null.</exception>
        public virtual async Task<MigrationSummary<TMigrationInfo>> MigrateAsync(Assembly migrationAssembly, DateTime now)
        {
            var migrationPlan = await GenerateMigrationPlanAsync(migrationAssembly);
            return await ApplyMigrationsAsync(migrationPlan.MigrationsToBeApplied, now);
        }

        /// <summary>
        /// Applies the provided migrations to the target system. The engine does not check
        /// if the specified migrations already exist.
        /// </summary>
        /// <param name="migrations">The list of migrations that should be applied.</param>
        /// <param name="now">
        /// The current time when the migration engine starts to execute. Please use a UTC time stamp if
        /// possible.
        /// </param>
        /// <returns>A summary of all migrations that have been applied in this run.</returns>
        public virtual async Task<MigrationSummary<TMigrationInfo>> ApplyMigrationsAsync(List<TMigration>? migrations, DateTime now)
        {
            if (migrations.IsNullOrEmpty())
                return MigrationSummary<TMigrationInfo>.Empty;

            var appliedMigrations = new List<TMigrationInfo>();
            foreach (var migration in migrations)
            {
                var migrationInfo = CreateMigrationInfo(migration, now);
                TSession? migrationSession = default;
                try
                {
                    migrationSession = SessionFactory.CreateSessionForMigration(migration);
                    await migration.ApplyAsync(migrationSession);
                    await migrationSession.StoreMigrationInfoAsync(migrationInfo);
                    appliedMigrations.Add(migrationInfo);
                    await migrationSession.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    var error = new MigrationError<TMigrationInfo>(migrationInfo, exception);
                    return new MigrationSummary<TMigrationInfo>(error, appliedMigrations);
                }
                finally
                {
                    migrationSession?.Dispose();
                }
            }

            return new MigrationSummary<TMigrationInfo>(appliedMigrations);
        }
    }
}