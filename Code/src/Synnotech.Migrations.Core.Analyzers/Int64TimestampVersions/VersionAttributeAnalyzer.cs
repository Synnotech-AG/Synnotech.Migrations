using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MigrationVersionAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMigrationClassForVersionAttribute, SymbolKind.NamedType);
        }

        private void AnalyzeMigrationClassForVersionAttribute(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol) context.Symbol;

            if (!typeSymbol.IsInt64TimestampMigration())
                return;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
    }
}