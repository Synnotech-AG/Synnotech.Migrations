using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class RoslynExtensions
    {
        public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(this DiagnosticAnalyzer analyzer, string code, [CallerMemberName] string? projectName = null)
        {
            projectName = projectName.MustNotBeNull(nameof(projectName));

            var (_, compilation) = await CreateInMemoryProject(projectName, code);
            return await compilation.WithAnalyzers(ImmutableArray.Create(analyzer))
                                    .GetAllDiagnosticsAsync();
        }

        public static async Task<string> ApplyFixAsync(this CodeFixProvider codeFixProvider,
                                                       string code,
                                                       DiagnosticAnalyzer analyzer,
                                                       [CallerMemberName] string? projectName = null)
        {
            projectName = projectName.MustNotBeNull(nameof(projectName));

            var (document, compilation) = await CreateInMemoryProject(projectName, code);

            var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create(analyzer))
                                               .GetAllDiagnosticsAsync();
            var codeActions = new List<CodeAction>();
            var context = new CodeFixContext(document,
                                             diagnostics[0],
                                             (action, diagnostic) => codeActions.Add(action),
                                             CancellationToken.None);
            await codeFixProvider.RegisterCodeFixesAsync(context);
            document = (await codeActions[0].GetOperationsAsync(CancellationToken.None)).OfType<ApplyChangesOperation>()
                                                                                        .Single()
                                                                                        .ChangedSolution
                                                                                        .GetDocument(document.Id);
            return (await document!.GetSyntaxRootAsync())!.GetText().ToString();
        }

        public static async Task<(Document, Compilation)> CreateInMemoryProject(string projectName, string code)
        {
            var references = ReferenceAssemblies.Net50.ToList();
            references.AddRange(new[]
            {
                MetadataReference.CreateFromFile(typeof(Throw).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(BaseMigration).Assembly.Location),
            });

            var projectId = ProjectId.CreateNewId(projectName);
            var codeFileName = $"{projectName}Source.cs";
            var codeFileId = DocumentId.CreateNewId(projectId, codeFileName);
            var project = new AdhocWorkspace().CurrentSolution
                                              .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                                              .AddMetadataReferences(projectId, references)
                                              .AddDocument(codeFileId, codeFileName, SourceText.From(code))
                                              .AddBaseMigrationClass(projectId)
                                              .GetProject(projectId)!
                                              .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var document = project.GetDocument(codeFileId)!;
            var compilation = (await project.GetCompilationAsync())!;

            return (document, compilation);
        }

        private static Solution AddBaseMigrationClass(this Solution solution, ProjectId projectId)
        {
            const string fileName = "Migration.cs";
            const string code = @"
using Synnotech.Migrations.Core.Int64TimestampVersions;

namespace MyProject.DataAccess.Migrations
{
    public abstract class Migration : BaseMigration { }
}";
            return solution.AddDocument(DocumentId.CreateNewId(projectId, fileName),
                                        fileName,
                                        SourceText.From(code));
        }
    }
}