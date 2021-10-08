using Synnotech.Migrations.Core;
using Xunit.Abstractions;

namespace Synnotech.Migrations.Linq2Db.Tests
{
    public static class Extensions
    {
        public static void LogSummary<T>(this ITestOutputHelper output, MigrationSummary<T> summary)
        {
            if (summary.TryGetAppliedMigrations(out var appliedMigrations))
            {
                foreach (var appliedMigration in appliedMigrations)
                {
                    output.WriteLine(appliedMigration!.ToString());
                }
            }

            output.WriteLine(string.Empty);

            if (summary.TryGetError(out var error))
                output.WriteLine(error.Exception.ToString());
        }
    }
}