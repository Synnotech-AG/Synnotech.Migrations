using System.Data.Entity;

namespace Synnotech.Migrations.EntityFramework;

/// <summary>
/// This interface must be implemented by your <see cref="DbContext" />. It represents
/// a database context that contains a table with metadata about applied migrations.
/// </summary>
/// <typeparam name="TMigrationInfo">The type that represents the migration info in the database.</typeparam>
public interface IHasMigrationInfos<TMigrationInfo>
    where TMigrationInfo : class
{
    /// <summary>
    /// The MigrationInfo DbSet that represents the migrations applied onto the database.
    /// </summary>
    DbSet<TMigrationInfo> MigrationInfos { get; }
}