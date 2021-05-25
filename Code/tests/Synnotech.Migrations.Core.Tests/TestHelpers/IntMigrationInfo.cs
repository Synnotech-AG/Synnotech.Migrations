namespace Synnotech.Migrations.Core.Tests.TestHelpers
{
    public sealed class IntMigrationInfo : IHasMigrationVersion<int>
    {
        public IntMigrationInfo(int migrationVersion) => MigrationVersion = migrationVersion;

        public int MigrationVersion { get; }

        public int GetMigrationVersion() => MigrationVersion;

        public static implicit operator IntMigrationInfo(int value) => new (value);
    }
}