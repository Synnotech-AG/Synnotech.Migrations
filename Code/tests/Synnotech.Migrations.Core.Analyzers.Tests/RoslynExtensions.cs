using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Basic.Reference.Assemblies;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Microsoft.CodeAnalysis;
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

        private static string FindNetstandardReferenceAssembly()
        {
            var netstandardAssembly = AppDomain.CurrentDomain
                                               .GetAssemblies()
                                               .First(assembly => assembly.GetName().Name == "netstandard");
            var dotnetFolder = Directory.GetParent(netstandardAssembly.Location)
                                        .FindParentDirectory("dotnet");

            if (dotnetFolder == null)
                throw new InvalidStateException($"The netstandard DLL is not located in the dotnet folder, but in \"{netstandardAssembly}\"");

            return Path.Combine(dotnetFolder.FullName, "packs", "NETStandard.Library.Ref", "2.1.0", "ref", "netstandard2.1", "netstandard.dll");
        }

        private static DirectoryInfo? FindParentDirectory(this DirectoryInfo? directoryInfo, string name)
        {
            if (directoryInfo == null)
                return null;

            var currentDirectory = directoryInfo.Parent;
            while (currentDirectory != null)
            {
                if (currentDirectory.Name == name)
                    return currentDirectory;

                currentDirectory = currentDirectory.Parent;
            }

            return null;
        }
    }
}