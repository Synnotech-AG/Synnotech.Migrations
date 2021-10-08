using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public DateTime? PredefinedDateTime { get; set; }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax compilationUnit)
                return;

            var diagnostic = context.Diagnostics[0];
            var targetNode = compilationUnit.FindNode(diagnostic.Location.SourceSpan);
            var classSyntax = (ClassDeclarationSyntax) targetNode;

            var descriptorTitle = diagnostic.Descriptor.Title.ToString();
            var iso8601StringTimestampTitle = descriptorTitle + " as ISO 8601 string timestamp";
            context.RegisterCodeFix(CodeAction.Create(iso8601StringTimestampTitle,
                                                      _ => AddMigrationVersionAttributeInIso8601TimestampFormat(context.Document, compilationUnit, classSyntax),
                                                      iso8601StringTimestampTitle),
                                    diagnostic);
            var int64TimestampTitle = descriptorTitle + " as Int64 timestamp";
            context.RegisterCodeFix(CodeAction.Create(int64TimestampTitle,
                                                      _ => AddMigrationVersionAttributeInInt64TimestampFormat(context.Document, compilationUnit, classSyntax),
                                                      int64TimestampTitle),
                                    diagnostic);
        }

        private Task<Document> AddMigrationVersionAttributeInIso8601TimestampFormat(Document document, CompilationUnitSyntax syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var timestamp = GetDateTime().ToIso8601UtcString();
            return InsertAttributeAndUsingStatement(document, syntaxRoot, classDeclarationSyntax, timestamp);
        }

        private Task<Document> AddMigrationVersionAttributeInInt64TimestampFormat(Document document, CompilationUnitSyntax syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax)
        {
            var timestamp = GetDateTime().ToInt64Timestamp();
            return InsertAttributeAndUsingStatement(document, syntaxRoot, classDeclarationSyntax, timestamp);
        }

        private static Task<Document> InsertAttributeAndUsingStatement<T>(Document document, CompilationUnitSyntax syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax, T timestamp)
        {
            syntaxRoot = InsertMigrationVersionAttribute(syntaxRoot, classDeclarationSyntax, timestamp);
            syntaxRoot = InsertUsingStatementIfNecessary(syntaxRoot);
            return Task.FromResult(document.WithSyntaxRoot(syntaxRoot));
        }

        private DateTime GetDateTime() => PredefinedDateTime ?? DateTime.UtcNow;

        private static CompilationUnitSyntax InsertMigrationVersionAttribute<T>(CompilationUnitSyntax syntaxRoot, ClassDeclarationSyntax classDeclarationSyntax, T timestamp)
        {
            SyntaxToken literal;
            SyntaxKind syntaxKind;
            if (typeof(T) == typeof(long))
            {
                var value = Unsafe.As<T, long>(ref timestamp);
                literal = Literal(value);
                syntaxKind = SyntaxKind.NumericLiteralExpression;
            }
            else if (typeof(T) == typeof(string))
            {
                var value = Unsafe.As<T, string>(ref timestamp);
                literal = Literal(value);
                syntaxKind = SyntaxKind.StringLiteralExpression;
            }
            else
            {
                throw new ArgumentException("timestamp can either be a string or a long value", nameof(timestamp));
            }

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
                                                    LiteralExpression(syntaxKind, literal))))))))
                   .NormalizeWhitespace();

            syntaxRoot = syntaxRoot.ReplaceNode(classDeclarationSyntax, newClassDeclarationSyntax)
                                   .NormalizeWhitespace();
            return syntaxRoot;
        }

        private static CompilationUnitSyntax InsertUsingStatementIfNecessary(CompilationUnitSyntax syntaxRoot)
        {
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

            return syntaxRoot;
        }
    }
}