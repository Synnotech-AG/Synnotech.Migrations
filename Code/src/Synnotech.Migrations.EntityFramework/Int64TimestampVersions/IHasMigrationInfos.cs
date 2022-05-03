using System.Data.Entity;

namespace Synnotech.Migrations.EntityFramework.Int64TimestampVersions;

/// <summary>
/// This interface must be implemented by your <see cref="DbContext" />. It represents
/// a database context that contains a table with metadata about applied migrations.
/// As the metadata type, <see cref="MigrationInfo" /> is used.
/// </summary>
public interface IHasMigrationInfos : IHasMigrationInfos<MigrationInfo> { }