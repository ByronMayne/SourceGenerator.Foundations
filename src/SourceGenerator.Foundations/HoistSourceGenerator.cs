using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SGF.Models;
using SGF.Templates;
using System.Runtime.CompilerServices;

namespace SGF
{
    /// <summary>
    /// Most basic generator used to generate base class for all the generator
    /// classes defined within the project
    /// </summary>
    [Generator]
    internal class HoistSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            
            var provider = context.SyntaxProvider
                 .CreateSyntaxProvider(
                     predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                     transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                 .Where(static m => m is not null);

            context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, AddCoreTypes);
            context.RegisterSourceOutput(provider,
                static (spc, source) => Execute(source!, spc));
        }

        private void AddCoreTypes(SourceProductionContext context, AnalyzerConfigOptionsProvider optionsProvider)
        {
            string @namespace = "SGF";
            context.AddSource("System_Runtime_CompilerServices_ModuleInitializerAttribute.g.cs", ModuleInitializerTemplate.Render());
            context.AddSource("SgfSourceGeneratorHoist.g.cs", SourceGeneratorHoistBase.RenderTemplate(@namespace));
            context.AddSource("SgfAssemblyResolver.g.cs", AssemblyResolverTemplate.Render(@namespace));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode s)
            => s is ClassDeclarationSyntax classDeclarationSyntax &&
                classDeclarationSyntax.AttributeLists.Count > 0;

        private static SourceGeneratorDataModel? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the [EnumExtensions] attribute?
                    if (fullName == "SGF.SgfGeneratorAttribute")
                    {
                        // return the enum. Implementation shown in section 7.
                        return SourceGeneratorDataModel.Create(classDeclarationSyntax, context.SemanticModel);
                    }
                }
            }

            return null;
        }

        private static void Execute(SourceGeneratorDataModel dataModel, SourceProductionContext context)
        {
            SourceText classDefinition = SourceGeneratorHostImpl.RenderTemplate(dataModel);
            context.AddSource($"Sgf{dataModel.ClassName}Host.g.cs", classDefinition);
        }
    }
}
