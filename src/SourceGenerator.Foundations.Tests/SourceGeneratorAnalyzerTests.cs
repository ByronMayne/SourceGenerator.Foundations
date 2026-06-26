using SGF;
using SGF.Analyzer;
using SGF.Analyzer.Rules;
using Xunit.Abstractions;

namespace SourceGenerator.Foundations.Tests
{
    public class SourceGeneratorAnalyzerTests : CompilationTestBase
    {
        public SourceGeneratorAnalyzerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AddAnalyzer<SourceGeneratorAnalyzer>();
        }

        [Fact]
        public async Task Generates_Without_Default_Constructor_Emit_SGF1003()
        {
            string source = """
                using SGF;
                namespace Yellow
                {
                    [SgfGenerator]
                    public class CustomGenerator : IncrementalGenerator
                    {
                        public CustomGenerator(string name) : base(name)
                        {}
                        public override void OnInitialize(SgfInitializationContext context)
                        {}
                    }
                }
                """;
            await ComposeAsync([source],
                assertDiagnostics: [
                    DiagnosticAsserts.TriggersRule<RequireDefaultConstructorRule>()
                ]);
        }

        [Fact]
        public async Task Abstract_Generators_Dont_Trigger_SGF1003()
        {
            string source = """
                using SGF;
                namespace Yellow
                {
                    [SgfGenerator]
                    public abstract class CustomGenerator : IncrementalGenerator
                    {
                        public CustomGenerator(string name) : base(name) 
                        {
                        }

                        public override void OnInitialize(SgfInitializationContext context)
                        {
                        }
                    }
                }
                """;
            await ComposeAsync([source],
                assertDiagnostics: [
                    DiagnosticAsserts.NoErrors()
                ]);
        }
    }
}
