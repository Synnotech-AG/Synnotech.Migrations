using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Synnotech.Migrations.Core.Analyzers.Tests
{
    public static class EqualsQualifiedNameTests
    {
        [Fact]
        public static void EqualsSameNamespace()
        {
            NameSyntax syntax =
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("Synnotech"),
                            IdentifierName("Migrations")),
                        IdentifierName("Core")),
                    IdentifierName("Int64TimestampVersions"));

            syntax.EqualsInt64TimestampsNamespace().Should().BeTrue();
        }

        [Fact]
        public static void ParentNamespaceNotEqual()
        {
            NameSyntax syntax =
                QualifiedName(
                    QualifiedName(
                        IdentifierName("Synnotech"),
                        IdentifierName("Migrations")),
                    IdentifierName("Core"));

            syntax.EqualsInt64TimestampsNamespace().Should().BeFalse();
        }

        [Fact]
        public static void SubNamespaceNotEqual()
        {
            NameSyntax syntax =
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("Synnotech"),
                                IdentifierName("Migrations")),
                            IdentifierName("Core")),
                        IdentifierName("Int64TimestampVersions")),
                    IdentifierName("SomeSubNamespace"));

            syntax.EqualsInt64TimestampsNamespace().Should().BeFalse();
        }

        [Fact]
        public static void TotallyDifferentNamespaceShouldNotBeEqual()
        {
            NameSyntax syntax =
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Collections")),
                    IdentifierName("Generic"));

            syntax.EqualsInt64TimestampsNamespace().Should().BeFalse();
        }
    }
}