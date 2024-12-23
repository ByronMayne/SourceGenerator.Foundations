using Microsoft.CodeAnalysis.CSharp.Analyzers;
using SGF;
using Xunit.Abstractions;

namespace SourceGenerator.Foundations.Tests
{
    /// <summary>
    /// Runs Roslyns analyzers to validate that code that was added
    /// by the <see cref="HoistSourceGenerator"/> does not cause 
    /// errors due to `EnforceExtendedAnalyzerRules`
    /// </summary>
    public class ExtendedAnalyzerRuleTests : CompiliationTestBase
    {
        public ExtendedAnalyzerRuleTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AnalyzerOptions.EnforceExtendedAnalyzerRules = true;

            AddGenerator<HoistSourceGenerator>();
            AddAnalyzer<CSharpSymbolIsBannedInAnalyzersAnalyzer>();
        }

        [Fact]
        public Task No_Banned_Apis_In_Source()
        {
            string source = """
                using SGF;

                namespace Yellow
                {
                    [SgfGenerator]
                    public class CustomGenerator : IncrementalGenerator
                    {
                        public CustomGenerator() : base("Generator")
                        {}

                        public override void OnInitialize(SgfInitializationContext context)
                        {}
                    }
                }
                """;

            return ComposeAsync([source],
                assertDiagnostics: [
                    DiagnosticAsserts.NoErrors()
                ]);
        }
    }
}
