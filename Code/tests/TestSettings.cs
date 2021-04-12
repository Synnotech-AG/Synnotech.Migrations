using Microsoft.Extensions.Configuration;

namespace Synnotech.Migrations
{
    public static partial class TestSettings
    {
        static TestSettings()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("testsettings.json", true)
                                                      .AddJsonFile("testsettings.Development.json", true)
                                                      .Build();
        }

        public static IConfiguration Configuration { get; }
    }
}