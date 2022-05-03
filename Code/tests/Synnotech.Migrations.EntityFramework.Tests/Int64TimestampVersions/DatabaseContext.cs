using System.Data.Entity;
using Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

namespace Synnotech.Migrations.EntityFramework.Tests.Int64TimestampVersions;

public class DatabaseContext : DbContext, IHasMigrationInfoTable<MigrationInfo>
{
    static DatabaseContext()
    {
        Database.SetInitializer<DatabaseContext>(null);
    }

    public DatabaseContext(string connectionString) : base(connectionString) { }

#nullable disable
    public DbSet<MigrationInfo> MigrationInfos { get; set; }

    public DbSet<RockClimber> RockClimbers { get; set; }
#nullable enable

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMigrationInfo()
                    .Entity<RockClimber>()
                    .ToTable("RockClimbers")
                    .HasKey(climber => climber.Id);

        modelBuilder.Entity<RockClimber>()
                    .Property(contact => contact.Name)
                    .IsRequired();
    }
}