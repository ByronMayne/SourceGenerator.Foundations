using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace SGF.Analyzer.Rules
{
    /// <summary>
    /// Ensures that <see cref="IncrementalGenerator"/> types have a default
    /// constructor defined.
    /// </summary>
    internal class RequireDefaultConstructorRule : AnalyzerRule
    {
        public RequireDefaultConstructorRule() : base(CreateDescriptor())
        {
        }

        protected override void Analyze(ClassDeclarationSyntax classDeclaration)
        {
            ConstructorDeclarationSyntax[] constructors = classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .ToArray();

            if(constructors.Length == 0)
            {
                // Already a compiler error since you need to call the base class constructor
                return;
            }

            if(constructors.Any(c => c.ParameterList.Parameters.Count == 0))
            {
                // We have a default constructor 
                return;
            }


            Location location = classDeclaration.Identifier.GetLocation();
            ReportDiagnostic(location, classDeclaration.Identifier.Text);
        }

        private static DiagnosticDescriptor CreateDescriptor()
        {
            return new DiagnosticDescriptor("SGF1003",
                        "SourceGeneratorHasDefaultConstructor",
                        $"{{0}} is missing a default constructor",
                        "SourceGeneration",
                        DiagnosticSeverity.Error,
                        true,
                        "SGF Incremental Generators must have a default constructor otherwise they will not be run");
        }
    }
}
