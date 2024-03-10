using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SGF.Analyzer.Rules
{
    /// <summary>
    /// Ensures tha the <see cref="GeneratorAttribute"/> is not applied to 
    /// <see cref="IncrementalGenerator"/> as these types are not really <see cref="IIncrementalGenerator"/>
    /// and won't be pickedup by Roslyn. 
    /// </summary>
    internal class ProhibitGeneratorAttributeRule : AnalyzerRule
    {
        public ProhibitGeneratorAttributeRule() : base(CreateDescriptor())
        {

        }

        protected override void Analyze(ClassDeclarationSyntax classDeclaration)
        {
            
            if (TryGetAttribute(classDeclaration, nameof(GeneratorAttribute), out AttributeSyntax? attributeSyntax))
            {
                Location location = attributeSyntax!.GetLocation();
                ReportDiagnostic(location, classDeclaration.Identifier.Text);
            }
        }

        private static DiagnosticDescriptor CreateDescriptor()
            => new DiagnosticDescriptor("SGF1002",
                "Prohibit GeneratorAttribute",
                $"{{0}} has the {nameof(GeneratorAttribute)} which can't be applied to classes which are inheirting from the Generator Foundations type {nameof(IncrementalGenerator)}.",
                "SourceGeneration",
                DiagnosticSeverity.Error,
                true,
                $"Incremental Generators should not have the {nameof(GeneratorAttribute)} applied to them.");
    }
}
