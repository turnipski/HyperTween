using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

public static class TypeSymbolExtensions
{
    public static IEnumerable<MemberDeclarationSyntax> GetMemberSyntax(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetMembers()
            .SelectMany(symbol => symbol.DeclaringSyntaxReferences)
            .Select(reference => reference.GetSyntax())
            .Select(node => node.GetParentMemberDeclaration());
    }

    public static MemberDeclarationSyntax GetParentMemberDeclaration(this SyntaxNode syntaxNode)
    {
        while (true)
        {
            if (syntaxNode is MemberDeclarationSyntax memberDeclarationSyntax)
            {
                return memberDeclarationSyntax;
            }

            if (syntaxNode.Parent == null)
            {
                throw new InvalidOperationException($"No Parent MemberDeclarationSyntax found for: {syntaxNode.ToFullString()}");
            }

            syntaxNode = syntaxNode.Parent;
        }
    }

    public static string GetFullName(this ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
    }
    public static bool ImplementsInterface(this ITypeSymbol typeSymbol, string interfaceFullName)
    {
        return typeSymbol.Interfaces.Any(symbol => symbol.GetFullName() == interfaceFullName);
    }
    
    public static bool TryGetImplementedInterface(this ITypeSymbol typeSymbol, string interfaceFullName, out ITypeSymbol interfaceSymbol)
    {
        var nullableInterfaceSymbol = typeSymbol.AllInterfaces.FirstOrDefault(symbol => symbol.GetFullName() == interfaceFullName);

        if (nullableInterfaceSymbol != null)
        {
            interfaceSymbol = nullableInterfaceSymbol;
            return true;
        }

        // A bit weird, but we need to assign _something_...
        interfaceSymbol = typeSymbol;
        return false;
    }

    public static bool TryGetGenericTypeArguments(this ITypeSymbol typeSymbol, out ImmutableArray<ITypeSymbol> typeArgumentSymbols)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol || !namedTypeSymbol.IsGenericType)
        {
            return false;
        }

        typeArgumentSymbols = namedTypeSymbol.TypeArguments;
        return true;
    }
    
    public static AttributeData? GetAttributeData(this ISymbol typeSymbol, string fullyQualifiedAttributeName)
    {
        if (!typeSymbol.HasAttribute(fullyQualifiedAttributeName))
        {
            return null;
        }
        else
        {
            fullyQualifiedAttributeName = PrependGlobalIfMissing(fullyQualifiedAttributeName);
            return typeSymbol.GetAttributes()
                .First(attribute => attribute.AttributeClass.ToFullName() == fullyQualifiedAttributeName);
        }
    }
    
    public static ITypeSymbol GetExpressedType(this TypedConstant constant)
    {
        if (constant.Kind != TypedConstantKind.Type)
        {
            throw new ArgumentException("The TypedConstant is not of kind 'Type'.", nameof(constant));
        }

        if (constant.Value is ITypeSymbol typeSymbol)
        {
            return typeSymbol;
        }

        throw new InvalidOperationException("The TypedConstant did not contain a valid ITypeSymbol.");
    }
    
    static string PrependGlobalIfMissing(this string typeOrNamespaceName) =>
        !typeOrNamespaceName.StartsWith("global::") ? $"global::{typeOrNamespaceName}" : typeOrNamespaceName;
}