using System.Threading.Tasks;
using Xunit;

namespace Synnotech.SqlServer.Tests
{
    public static class DatabaseTests
    {
        [SkippableFact]
        public static async Task CreateDatabase()
        {
            var connectionString = TestSettings.GetConnectionString();

            await Database.TryDropDatabase(connectionString);
            await Database.DropAndCreateDatabaseAsync(connectionString);
        }
    }
}