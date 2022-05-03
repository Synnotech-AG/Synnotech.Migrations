using System.Data.Entity;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;

public class DatabaseContext : DbContext, IHasMigrationInfos
{
    static DatabaseContext() => Database.SetInitializer<DatabaseContext>(null);

    public DatabaseContext(string connectionString) : base(connectionString) { }

#nullable disable
    public DbSet<RockClimber> RockClimbers { get; set; }

    public DbSet<MigrationInfo> MigrationInfos { get; set; }
#nullable enable

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMigrationInfo();

        modelBuilder.Entity<RockClimber>()
                    .ToTable("RockClimbers")
                    .HasKey(climber => climber.Id)
                    .Property(climber => climber.Name)
                    .IsRequired();
    }
}