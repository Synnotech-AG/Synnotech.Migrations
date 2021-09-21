using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Synnotech.DatabaseAbstractions;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class RoslynExtensions
    {
        // TODO: reference netstandard 2.1 DLL, see https://stackoverflow.com/questions/58840995/roslyn-compilation-how-to-reference-a-net-standard-2-0-class-library
        public static readonly MetadataReference SystemRuntimeReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        public static readonly MetadataReference CompilationReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        public static readonly MetadataReference CSharpCompilationReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        public static readonly MetadataReference SynnotechMigrationsCoreReference = MetadataReference.CreateFromFile(typeof(IMigrationAttribute).Assembly.Location);
        public static readonly MetadataReference SynnotechDatabaseAbstractionsReference = MetadataReference.CreateFromFile(typeof(IAsyncSession).Assembly.Location);
        public static readonly MetadataReference LightGuardClausesReference = MetadataReference.CreateFromFile(typeof(Throw).Assembly.Location);
        public static readonly MetadataReference MicrosoftDependencyInjectionReference = MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location);

        public static IReadOnlyList<MetadataReference> AllReferences =
            new[]
            {
                SystemRuntimeReference,
                CompilationReference,
                CSharpCompilationReference,
                SynnotechMigrationsCoreReference,
                SynnotechDatabaseAbstractionsReference,
                LightGuardClausesReference,
                MicrosoftDependencyInjectionReference
            };

        public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(this DiagnosticAnalyzer analyzer, string code, [CallerMemberName] string? projectName = null)
        {
            projectName = projectName.MustNotBeNull(nameof(projectName));

            var (_, compilation) = await CreateInMemoryProject(projectName, code);
            return await compilation.WithAnalyzers(ImmutableArray.Create(analyzer))
                                    .GetAllDiagnosticsAsync();
        }

        public static async Task<(Document, Compilation)> CreateInMemoryProject(string projectName, string code)
        {
            var projectId = ProjectId.CreateNewId(projectName);
            var codeFileName = $"{projectName}Source.cs";
            var codeFileId = DocumentId.CreateNewId(projectId, codeFileName);
            var project = new AdhocWorkspace().CurrentSolution
                                              .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                                              .AddMetadataReferences(projectId, AllReferences)
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