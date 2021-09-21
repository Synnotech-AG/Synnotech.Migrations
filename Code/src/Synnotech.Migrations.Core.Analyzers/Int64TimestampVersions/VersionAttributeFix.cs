using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Adds the migration version attribute to the migration class.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class VersionAttributeFix : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.MissingMigrationVersionAttribute.Id);

        /// <inheritdoc />
        public override FixAllProvider? GetFixAllProvider() => null;

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (syntaxRoot == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var classSyntax = (ClassDeclarationSyntax) syntaxRoot.FindNode(diagnostic.Location.SourceSpan);

            var title = diagnostic.Descriptor.Title.ToString();
            context.RegisterCodeFix(CodeAction.Create(title,
                                                      _ => AddMigrationVersionAttribute(context.Document, syntaxRoot, classSyntax),
                                                      title),
                                    diagnostic);
        }

        private static Task<Document> AddMigrationVersionAttribute(Document document, SyntaxNode syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax)
        {
            throw new NotImplementedException();
        }
    }
}