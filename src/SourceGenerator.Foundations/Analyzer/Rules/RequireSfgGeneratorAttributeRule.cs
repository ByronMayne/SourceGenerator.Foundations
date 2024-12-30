using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SGF.Analyzer.Rules
{
    internal class RequireSfgGeneratorAttributeRule : AnalyzerRule
    {

        public RequireSfgGeneratorAttributeRule() : base(CreateDescriptor())
        {
        }

        protected override void Analyze(ClassDeclarationSyntax classDeclaration)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if ( !HasAttribute(classDeclaration, nameof(IncrementalGeneratorAttribute)) &&
                !HasAttribute(classDeclaration, nameof(SgfGeneratorAttribute)))
            {
                Location location = classDeclaration.Identifier.GetLocation();
                ReportDiagnostic(location, classDeclaration.Identifier.Text);
            }
#pragma warning restore CS0618 // Type or member is obsolete

        }

        private static DiagnosticDescriptor CreateDescriptor()
            => new DiagnosticDescriptor("SGF1001",
                "SGFGeneratorAttributeApplied",
                $"{{0}} is missing the {nameof(IncrementalGeneratorAttribute)}",
                "SourceGeneration",
                DiagnosticSeverity.Error,
                true,
                $"Source generators are required to have the attribute {nameof(IncrementalGeneratorAttribute)} applied to them otherwise the compiler won't invoke them",
                "https://github.com/ByronMayne/SourceGenerator.Foundations?tab=readme-ov-file#sgf1001");
    }
}
