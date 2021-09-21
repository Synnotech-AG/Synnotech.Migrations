using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions;
using Xunit;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class MissingMigrationVersionAttributeTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new MigrationVersionAttributeAnalyzer();

        private const string MissingMigrationAttributeCode = @"
namespace MyProject.DataAccess.Migrations
{
    public sealed class MyMigration : Migration { }
}";

        [Fact]
        public static async Task AnalyzeMissingAttribute()
        {
            var diagnostics = await Analyzer.AnalyzeAsync(MissingMigrationAttributeCode);

            diagnostics.Should().HaveCount(1);
            diagnostics[0].Descriptor.Should().BeSameAs(Descriptors.MissingMigrationVersionAttribute);
        }
    }
}