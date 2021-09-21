using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax compilationUnit)
                return;

            var diagnostic = context.Diagnostics[0];
            var classSyntax = (ClassDeclarationSyntax) compilationUnit.FindNode(diagnostic.Location.SourceSpan);

            var title = diagnostic.Descriptor.Title.ToString();
            context.RegisterCodeFix(CodeAction.Create(title,
                                                      _ => AddMigrationVersionAttribute(context.Document, compilationUnit, classSyntax),
                                                      title),
                                    diagnostic);
        }

        private static Task<Document> AddMigrationVersionAttribute(Document document, CompilationUnitSyntax syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var timestamp = DateTime.UtcNow.ToIso8601UtcString();

            // Add the attribute to the class
            var newClassDeclarationSyntax =
                classDeclarationSyntax
                   .AddAttributeLists(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                        IdentifierName("MigrationVersion"))
                                   .WithArgumentList(
                                        AttributeArgumentList(
                                            SingletonSeparatedList(
                                                AttributeArgument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(timestamp)))))))))
                   .NormalizeWhitespace();

            // Check if the using directive must be inserted
            if (syntaxRoot.DescendantNodes()
                          .OfType<UsingDirectiveSyntax>()
                          .All(usingDirective => !usingDirective.Name.EqualsInt64TimestampsNamespace()))
            {
                syntaxRoot =
                    syntaxRoot
                       .AddUsings(
                            UsingDirective(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("Synnotech"),
                                            IdentifierName("Migrations")),
                                        IdentifierName("Core")),
                                    IdentifierName("Int64TimestampVersions"))))
                       .NormalizeWhitespace();
            }

            // Insert the new class declaration into the syntax root
            return Task.FromResult(
                document.WithSyntaxRoot(
                    syntaxRoot.ReplaceNode(classDeclarationSyntax, newClassDeclarationSyntax)));
        }
    }
}