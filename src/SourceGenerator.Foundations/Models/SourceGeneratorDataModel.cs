using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SGF.Models
{
    internal class SourceGeneratorDataModel
    {
        public string ClassName { get; }
        public string Namespace { get; }
        public string AccessModifier { get; }
        public string QualifiedName { get; }

        public SourceGeneratorDataModel(string name, string @namespace, string accessModifier)
        {
            ClassName = name;
            Namespace = @namespace;
            AccessModifier = accessModifier;
            QualifiedName = string.IsNullOrEmpty(Namespace) ? ClassName : $"{Namespace}.{ClassName}";
        }

        public static SourceGeneratorDataModel? Create(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel)
        {
            INamedTypeSymbol? typeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (typeSymbol == null)
            {
                return null;
            }

            return new SourceGeneratorDataModel(
                name: typeSymbol.Name,
                @namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
                accessModifier: typeSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal"
             );
        }
    }
}
