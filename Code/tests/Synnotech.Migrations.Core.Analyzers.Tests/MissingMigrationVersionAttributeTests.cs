using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions;
using Xunit;
using Xunit.Abstractions;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public sealed class MissingMigrationVersionAttributeTests
    {
        public MissingMigrationVersionAttributeTests(ITestOutputHelper output) => Output = output;

        private static DiagnosticAnalyzer Analyzer { get; } = new MigrationVersionAttributeAnalyzer();

        private ITestOutputHelper Output { get; }

        private const string MissingMigrationAttributeCode = @"
namespace MyProject.DataAccess.Migrations
{
    public sealed class MyMigration : Migration { }
}";

        private const string FixedMigrationAttributeCode = @"
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace MyProject.DataAccess.Migrations
{
    [MigrationVersion(""2021-10-01T00:25:30"")]
    public sealed class MyMigration : Migration { }
}";

        [Fact]
        public async Task AnalyzeMissingAttribute()
        {
            var diagnostics = await Analyzer.AnalyzeAsync(MissingMigrationAttributeCode);

            foreach (var diagnostic in diagnostics)
            {
                Output.WriteLine(diagnostic.ToString());
            }

            diagnostics.Should().HaveCount(1);
            diagnostics[0].Descriptor.Should().BeSameAs(Descriptors.MissingMigrationVersionAttribute);
        }

        [Fact]
        public async Task FixMissingAttribute()
        {
            var codeFix = new VersionAttributeFix();
            var resultingCode = await codeFix.ApplyFixAsync(MissingMigrationAttributeCode, Analyzer);

            Output.WriteLine(resultingCode);

            resultingCode.Should().Contain("using Synnotech.Migrations.Core.Int64TimestampVersions;");
            resultingCode.Should().Contain("[MigrationVersion(\"");
        }

        [Fact]
        public async Task AnalyzerShouldNotTriggerWhenAttributeIsPresent()
        {
            var diagnostics = await Analyzer.AnalyzeAsync(FixedMigrationAttributeCode);

            diagnostics.Should().BeEmpty();
        }
    }
}