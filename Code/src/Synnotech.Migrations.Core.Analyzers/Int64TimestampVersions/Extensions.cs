using Light.GuardClauses;
using Microsoft.CodeAnalysis;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    /// <summary>
    /// Provides extensions methods to check for Int64 timestamp migrations that have
    /// no migration version attribute.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Checks if the specified type symbol is a class, derives from Int64TimestampVersions.BaseMigration and
        /// has no attributes applied to it.
        /// </summary>
        public static bool IsInt64TimestampMigrationWithoutVersionAttribute(this INamedTypeSymbol typeSymbol) =>
            typeSymbol.IsDerivingFromBaseMigration() && typeSymbol.IsWithoutMigrationVersionAttribute();

        /// <summary>
        /// Checks if the specified type symbol is a class and derives from Synnotech.Migrations.Core.Int64TimestampVersions.BaseMigration.
        /// </summary>
        public static bool IsDerivingFromBaseMigration(this INamedTypeSymbol typeSymbol)
        {
            typeSymbol.MustNotBeNull(nameof(typeSymbol));

            if (typeSymbol.TypeKind != TypeKind.Class)
                return false;

            if (typeSymbol.IsAbstract)
                return false;

            var currentBaseType = typeSymbol.BaseType;
            while (currentBaseType != null)
            {
                if (currentBaseType.Name != Constants.BaseMigrationClassName)
                    goto Continue;

                var @namespace = currentBaseType.ContainingNamespace;
                if (@namespace.IsInt64TimestampVersionsNamespace())
                    return true;
                
                Continue:
                currentBaseType = currentBaseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified type has the migration version attribute applied to it.
        /// </summary>
        public static bool IsWithoutMigrationVersionAttribute(this INamedTypeSymbol typeSymbol)
        {
            typeSymbol.MustNotBeNull(nameof(typeSymbol));

            var attributeData = typeSymbol.GetAttributes();
            return attributeData.Length == 0;
        }

        private static bool IsInt64TimestampVersionsNamespace(this INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol.Name != "Int64TimestampVersions")
                return false;

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (namespaceSymbol.Name != "Core")
                return false;

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (namespaceSymbol.Name != "Migrations")
                return false;

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (namespaceSymbol.Name != "Synnotech")
                return false;

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            return namespaceSymbol.Name == "";
        }
    }
}