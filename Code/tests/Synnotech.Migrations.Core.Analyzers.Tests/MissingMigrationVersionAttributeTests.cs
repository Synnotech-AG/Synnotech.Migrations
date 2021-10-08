using System;
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
    [MigrationVersion(""2021-10-01T00:25:30Z"")]
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
        public async Task FixMissingAttributeWithIso8601String()
        {
            var now = DateTime.UtcNow;
            var codeFix = new VersionAttributeFix { PredefinedDateTime = now };
            var resultingCode = await codeFix.ApplyFixAsync(MissingMigrationAttributeCode, Analyzer);

            Output.WriteLine(resultingCode);

            var iso8601Timestamp = now.ToIso8601UtcString();
            var expectedCode =
$@"using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace MyProject.DataAccess.Migrations
{{
    [MigrationVersion(""{iso8601Timestamp}"")]
    public sealed class MyMigration : Migration
    {{
    }}
}}";
            resultingCode.Should().Be(expectedCode);
        }

        [Fact]
        public async Task FixMissingAttributeWithInt64Timestamp()
        {
            var now = DateTime.UtcNow;
            var codeFix = new VersionAttributeFix { PredefinedDateTime = now };
            var resultingCode = await codeFix.ApplyFixAsync(MissingMigrationAttributeCode, Analyzer, 1);

            Output.WriteLine(resultingCode);

            var int64Timestamp = now.ToInt64Timestamp();
            var expectedCode =
$@"using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace MyProject.DataAccess.Migrations
{{
    [MigrationVersion({int64Timestamp}L)]
    public sealed class MyMigration : Migration
    {{
    }}
}}";
            resultingCode.Should().Be(expectedCode);
        }

        [Fact]
        public async Task AnalyzerShouldNotTriggerWhenAttributeIsPresent()
        {
            var diagnostics = await Analyzer.AnalyzeAsync(FixedMigrationAttributeCode);

            diagnostics.Should().BeEmpty();
        }
    }
}