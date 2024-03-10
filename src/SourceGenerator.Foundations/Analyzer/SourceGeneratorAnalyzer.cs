using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SGF.Analyzer.Rules;
using System.Collections.Immutable;
using System.Linq;

namespace SGF.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SourceGeneratorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc cref="DiagnosticAnalyzer"/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        internal ImmutableArray<AnalyzerRule> Rules { get; }

        public SourceGeneratorAnalyzer()
        {
            Rules = new AnalyzerRule[]
            {
                new RequireSfgGeneratorAttributeRule(),
                new ProhibitGeneratorAttributeRule(),
                new RequireDefaultConstructorRule()
            }.ToImmutableArray();
            SupportedDiagnostics = Rules.Select(r => r.Descriptor).ToImmutableArray();
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(CheckForAttribute, SyntaxKind.ClassDeclaration);
        }

        private void CheckForAttribute(SyntaxNodeAnalysisContext context)
        {
            SemanticModel semanticModel = context.SemanticModel;
            ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol? symbolInfo = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classDeclaration.BaseList == null || symbolInfo == null) return;
            if (!IsIncrementalGenerator(symbolInfo)) return;

            foreach(AnalyzerRule rule in Rules)
            {
                rule.Invoke(context, classDeclaration);
            }
        }

        /// <summary>
        /// Returns back if the type inheirts from <see cref="IncrementalGenerator"/> or not
        /// </summary>
        /// <param name="typeSymbol">The type to check</param>
        /// <returns>True if it does and false if it does not</returns>
        private static bool IsIncrementalGenerator(INamedTypeSymbol? typeSymbol)
        {
            while (typeSymbol != null)
            {
                if (string.Equals(typeSymbol.ToDisplayString(), "SGF.IncrementalGenerator"))
                {
                    return true;
                }

                typeSymbol = typeSymbol.BaseType;
            }

            return false;
        }
    }
}
