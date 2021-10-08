using Microsoft.CodeAnalysis;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Provides descriptors for all analyzers / code fixes for
    /// </summary>
    public static class Descriptors
    {
        /// <summary>
        /// Gets the migrations category name.
        /// </summary>
        public const string Category = "Migrations";

        /// <summary>
        /// Gets the diagnostic descriptor for rule SM1000
        /// </summary>
        public static readonly DiagnosticDescriptor MissingMigrationVersionAttribute =
            new ("SM1000",
                 "Apply the Int64Timestamp Migration Version",
                 "This migration will not be picked up by the migration engine because the migration version attribute is missing",
                 Category,
                 DiagnosticSeverity.Info,
                 true);
    }
}