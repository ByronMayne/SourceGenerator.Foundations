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
            if (!HasAttribute(classDeclaration, nameof(SgfGeneratorAttribute)))
            {
                Location location = classDeclaration.Identifier.GetLocation();
                ReportDiagnostic(location, classDeclaration.Identifier.Text);
            }
        }

        private static DiagnosticDescriptor CreateDescriptor()
            => new DiagnosticDescriptor("SGF1001",
                "SGFGeneratorAttributeApplied",
                $"{{0}} is missing the {nameof(SgfGeneratorAttribute)}",
                "SourceGeneration",
                DiagnosticSeverity.Error,
                true,
                $"Source generators are required to have the attribute {nameof(SgfGeneratorAttribute)} applied to them otherwise the compiler won't invoke them");
    }
}
