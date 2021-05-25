using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents the most generic version of an asynchronous migration engine. It searches for migrations in
    /// the target assembly, compares them to the latest applied migration of the target system
    /// and then applies the pending migrations.
    /// </summary>
    /// <typeparam name="TMigrationVersion">The type that represents a migration version. It must be equatable and comparable.</typeparam>
    /// <typeparam name="TMigration">
    /// The base class that identifies all migrations. Must implement <see cref="IMigration{TContext}" /> and
    /// <see cref="IHasMigrationVersion{TMigrationVersion}"/>.
    /// </typeparam>
    /// <typeparam name="TMigrationAttribute">
    /// The type that represents the attribute being applied to migrations to indicate their version.
    /// Must implement <see cref="IMigrationAttribute"/> and <see cref="IHasMigrationVersion{TMigrationVersion}"/>.
    /// </typeparam>
    /// <typeparam name="TMigrationInfo">
    /// That type whose instances are stored in the target system to indicate which
    /// migrations already have been applied.
    /// </typeparam>
    /// <typeparam name="TMigrationContext">The type whose instances are passed to each migration when they are applied.</typeparam>
    public class MigrationEngine<TMigrationVersion, TMigration, TMigrationAttribute, TMigrationInfo, TMigrationContext>
        where TMigrationVersion : IEquatable<TMigrationVersion>, IComparable<TMigrationVersion>
        where TMigration : IMigration<TMigrationContext>, IHasMigrationVersion<TMigrationVersion>
        where TMigrationAttribute : Attribute, IMigrationAttribute, IHasMigrationVersion<TMigrationVersion>
        where TMigrationInfo : IHasMigrationVersion<TMigrationVersion>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationEngine{TMigrationVersion,TMigration,TMigrationAttribute,TMigrationInfo,TMigrationContext}"/>.
        /// </summary>
        /// <param name="sessionFactory">The factory that is used to create sessions which are used to access the target system.</param>
        /// <param name="migrationFactory">The factory that is used to instantiate migrations.</param>
        /// <param name="createMigrationInfo">The delegate that is used to instantiate migration infos.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public MigrationEngine(ISessionFactory<TMigrationInfo, TMigration, TMigrationContext> sessionFactory,
                               IMigrationFactory<TMigration> migrationFactory,
                               Func<TMigration, DateTime, TMigrationInfo> createMigrationInfo)
        {
            SessionFactory = sessionFactory.MustNotBeNull(nameof(sessionFactory));
            MigrationFactory = migrationFactory.MustNotBeNull(nameof(migrationFactory));
            CreateMigrationInfo = createMigrationInfo.MustNotBeNull(nameof(createMigrationInfo));
        }

        /// <summary>
        /// Gets the factory used to create sessions to the target system.
        /// </summary>
        protected ISessionFactory<TMigrationInfo, TMigration, TMigrationContext> SessionFactory { get; }

        /// <summary>
        /// Gets the factory that is used to instantiate migrations from types.
        /// </summary>
        protected IMigrationFactory<TMigration> MigrationFactory { get; }

        /// <summary>
        /// Gets the delegate that is used to create migration infos.
        /// </summary>
        protected Func<TMigration, DateTime, TMigrationInfo> CreateMigrationInfo { get; }

        /// <summary>
        /// Generates a plan that contains information about the latest applied migration on the target system
        /// and the migrations that need to be applied.
        /// </summary>
        /// <param name="assembliesContainingMigrations">The assemblies that will be searched for migration types.</param>
        /// <returns>A plan that describes the latest applied migration and the pending migrations.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembliesContainingMigrations"/> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assembliesContainingMigrations"/> is an empty array.</exception>
        /// <exception cref="Exception">A system-specific exception might occur when there are errors with the connection to the target system (e.g. a SqlException).</exception>
        public virtual async Task<MigrationPlan<TMigrationVersion, TMigrationInfo>> GetPlanForNewMigrationsAsync(params Assembly[] assembliesContainingMigrations)
        {
            assembliesContainingMigrations.MustNotBeNullOrEmpty(nameof(assembliesContainingMigrations));

            await using var session = await SessionFactory.CreateSessionForRetrievingLatestMigrationInfoAsync();

            var latestMigrationInfo = await session.GetLatestMigrationInfoAsync();

            var latestId = default(TMigrationVersion);
            if (latestMigrationInfo != null)
                latestId = latestMigrationInfo.GetMigrationVersion();

            var migrationsToBeApplied = PendingMigrations.DetermineNewMigrations<TMigrationVersion, TMigration, TMigrationAttribute>(latestId, assembliesContainingMigrations);
            return new MigrationPlan<TMigrationVersion, TMigrationInfo>(latestMigrationInfo, migrationsToBeApplied);
        }

        /// <summary>
        /// Determines and applies migrations to the target system. This is done by getting the
        /// latest applied migration info from the target system, picking the migrations
        /// that must be executed, and applying them to the target system.
        /// PLEASE NOTE: when migrations are applied, all exceptions are caught by the migration engine.
        /// Exceptions are not caught in the analysis phase beforehand (when the latest migration info is retrieved).
        /// This means that you must analyze the summary for errors (e.g. via <see cref="MigrationSummary{TMigrationInfo}.EnsureSuccess" />)
        /// to ensure that no errors occurred during a run.
        /// </summary>
        /// <param name="now">The current time when the migration engine starts to execute. Please use a UTC time stamp if possible.</param>
        /// <param name="assembliesContainingMigrations">The assemblies that will be searched for migration types.</param>
        /// <returns>A summary of all migrations that have been applied in this run and an optional error that might have occurred during a migration.</returns>
        /// /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembliesContainingMigrations"/> is null.</exception>
        /// <exception cref="EmptyCollectionException">Thrown when <paramref name="assembliesContainingMigrations"/> is an empty array.</exception>
        /// <exception cref="Exception">A system-specific exception might occur when there are errors with the connection to the target system (e.g. a SqlException).</exception>
        public virtual async Task<MigrationSummary<TMigrationInfo>> MigrateAsync(DateTime now, params Assembly[] assembliesContainingMigrations)
        {
            var migrationPlan = await GetPlanForNewMigrationsAsync(assembliesContainingMigrations);
            return await ApplyMigrationsAsync(migrationPlan.PendingMigrations, now);
        }

        /// <summary>
        /// Applies the provided migrations to the target system. The engine does not check
        /// if the specified migrations were already applied.
        /// PLEASE NOTE: this method will not throw. You must check the resulting summary for errors that might have occurred.
        /// </summary>
        /// <param name="pendingMigrations">The list of migrations that should be applied.</param>
        /// <param name="now">The current time when the migration engine starts to execute. Please use a UTC time stamp if possible.</param>
        /// <returns>A summary of all migrations that have been applied in this run.</returns>
        public virtual async Task<MigrationSummary<TMigrationInfo>> ApplyMigrationsAsync(List<PendingMigration<TMigrationVersion>>? pendingMigrations, DateTime now)
        {
            if (pendingMigrations.IsNullOrEmpty())
                return MigrationSummary<TMigrationInfo>.Empty;

            var appliedMigrations = new List<TMigrationInfo>(pendingMigrations.Count);
            for (var i = 0; i < pendingMigrations.Count; i++)
            {
                var pendingMigration = pendingMigrations[i];

                IMigrationSession<TMigrationContext, TMigrationInfo>? session = null;
                try
                {
                    var migration = MigrationFactory.CreateMigration(pendingMigration.MigrationType);
                    session = await SessionFactory.CreateSessionForMigrationAsync(migration);
                    await migration.ApplyAsync(session.Context);
                    var migrationInfo = CreateMigrationInfo(migration, now);
                    await session.StoreMigrationInfoAsync(migrationInfo);
                    await session.SaveChangesAsync();
                    appliedMigrations.Add(migrationInfo);
                }
                catch (Exception exception)
                {
                    var error = new MigrationError<TMigrationVersion>(pendingMigration.MigrationVersion, exception);
                    return new MigrationSummary<TMigrationInfo>(error, appliedMigrations);
                }
                finally
                {
                    if (session != null)
                        await session.DisposeAsync();
                }
            }

            return new MigrationSummary<TMigrationInfo>(appliedMigrations);
        }
    }
}