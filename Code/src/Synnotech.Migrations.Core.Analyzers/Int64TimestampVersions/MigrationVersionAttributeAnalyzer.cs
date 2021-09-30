using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Checks if a Int64Timestamp migration is missing the migration version attribute.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MigrationVersionAttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the <see cref="Descriptors.MissingMigrationVersionAttribute"/> descriptor in an immutable array.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptors.MissingMigrationVersionAttribute);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMigrationClassForVersionAttribute, SymbolKind.NamedType);
        }

        private static void AnalyzeMigrationClassForVersionAttribute(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol) context.Symbol;

            if (!typeSymbol.IsInt64TimestampMigrationWithoutVersionAttribute())
                return;

            var classDeclarationSyntax = typeSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingMigrationVersionAttribute, classDeclarationSyntax.GetLocation()));
        }
    }
}