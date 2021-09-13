using System;
using Microsoft.CodeAnalysis;

namespace Synnotech.Migrations.Core.Analyzers.Int64TimestampVersions
{
    public static class Extensions
    {
        public static bool IsInt64TimestampMigration(this INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Class)
                return false;

            throw new NotImplementedException();
        }
    }
}