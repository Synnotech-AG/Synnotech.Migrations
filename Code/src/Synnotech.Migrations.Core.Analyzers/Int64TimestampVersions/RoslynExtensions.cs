using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Provides extension methods for Roslyn.
    /// </summary>
    public static class RoslynExtensions
    {
        private static string[] Int64TimestampsNamespaceIdentifiers { get; } =
            { "Synnotech", "Migrations", "Core", "Int64TimestampVersions" };

        /// <summary>
        /// Checks if the specified name syntax is equal to "Synnotech.Migrations.Core.Int64TimestampVersions".
        /// </summary>
        public static bool EqualsInt64TimestampsNamespace(this NameSyntax nameSyntax) =>
            nameSyntax.EqualsQualifiedName(Int64TimestampsNamespaceIdentifiers);

        /// <summary>
        /// Checks if the specified name syntax equals the array of identifiers.
        /// </summary>
        /// <param name="nameSyntax">The name syntax to be checked.</param>
        /// <param name="identifiers">The strings that make up the full name.</param>
        public static bool EqualsQualifiedName(this NameSyntax nameSyntax, string[] identifiers)
        {
            if (nameSyntax == null)
                throw new ArgumentNullException(nameof(nameSyntax));
            if (identifiers == null)
                throw new ArgumentNullException(nameof(identifiers));
            if (identifiers.Length == 0)
                return false;

            var remainingIdentifiers = new ReadOnlySpan<string>(identifiers);
            return CheckIdentifierNamesRecursively(nameSyntax, ref remainingIdentifiers, 0);
        }

        private static bool CheckIdentifierNamesRecursively(NameSyntax currentNode, ref ReadOnlySpan<string> remainingIdentifiers, int recursionLevel)
        {
            if (currentNode is IdentifierNameSyntax identifierName)
            {
                if (remainingIdentifiers.Length == 0)
                    return false;

                var currentIdentifier = remainingIdentifiers[0];
                if (identifierName.Identifier.ValueText != currentIdentifier)
                    return false;

                remainingIdentifiers = remainingIdentifiers.Length > 1 ? remainingIdentifiers.Slice(1) : ReadOnlySpan<string>.Empty;
                return true;
            }

            if (currentNode is QualifiedNameSyntax qualifiedName)
            {
                if (!CheckIdentifierNamesRecursively(qualifiedName.Left, ref remainingIdentifiers, recursionLevel + 1))
                    return false;
                if (!CheckIdentifierNamesRecursively(qualifiedName.Right, ref remainingIdentifiers, recursionLevel + 1))
                    return false;
                    
                return recursionLevel != 0 || remainingIdentifiers.Length == 0;
            }

            return false;
        }
    }
}