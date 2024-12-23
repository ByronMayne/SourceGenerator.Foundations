using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SGF
{
    public class SgfAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        public SgfAnalyzerConfigOptions SharedOptions { get; }
        public override AnalyzerConfigOptions GlobalOptions => SharedOptions;

        public SgfAnalyzerConfigOptionsProvider()
        {
            SharedOptions = new SgfAnalyzerConfigOptions();
        }

        public SgfAnalyzerConfigOptionsProvider(SgfAnalyzerConfigOptions options)
        {
            SharedOptions = options;
        }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => GlobalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => GlobalOptions;
    }
}
