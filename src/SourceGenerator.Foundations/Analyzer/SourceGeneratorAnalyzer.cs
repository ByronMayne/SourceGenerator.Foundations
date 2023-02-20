using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace SGF.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SourceGeneratorAnalyzer : DiagnosticAnalyzer
    {
        public DiagnosticDescriptor GeneratorAttributeDescriptor { get; }

        /// <inheritdoc cref="DiagnosticAnalyzer"/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public SourceGeneratorAnalyzer()
        {
            GeneratorAttributeDescriptor = new DiagnosticDescriptor("sgf-generator-attribute-is-applied",
                "SourceGeneratorAttributeApplied",
                $"The class is missing the {nameof(GeneratorAttribute)} which is required for them to work.",
                "SourceGeneration",
                DiagnosticSeverity.Error,
                true,
                $"Source generators are required to have the attribute {nameof(GeneratorAttribute)} applied to them otherwise the compiler won't invoke them",
                "https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.generatorattribute?view=roslyn-dotnet-4.3.0");

            SupportedDiagnostics = new[] { GeneratorAttributeDescriptor }.ToImmutableArray();
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
        }
    }
}
