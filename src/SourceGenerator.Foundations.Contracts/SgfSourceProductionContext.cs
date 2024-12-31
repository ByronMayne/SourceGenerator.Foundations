using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading;

namespace SGF
{
    /// <summary>
    /// Wrapper around a <see cref="SourceProductionContext"/> used to help capture errors and report logs
    /// </summary>
    public struct SgfSourceProductionContext : ISgfSourceProductionContext
    {
        private readonly SourceProductionContext m_context;
        private readonly IncrementalGenerator m_generator;

        /// <inheritdoc/>
        public int SourceCount { get; private set; }

        /// <inheritdoc/>
        SourceProductionContext ISgfSourceProductionContext.OriginalContext => m_context;

        /// <summary>
        /// A token that will be canceled when generation should stop
        /// </summary>
        public CancellationToken CancellationToken => m_context.CancellationToken;

        internal SgfSourceProductionContext(SourceProductionContext context, IncrementalGenerator generator)
        {
            SourceCount = 0;
            m_generator = generator;
            m_context = context;

        }

        /// <inheritdoc/>
        public void AddSource(string hintName, string source)
            => AddSource(hintName, SourceText.From(source, Encoding.UTF8));

        /// <inheritdoc/>
        public void AddSource(string hintName, SourceText sourceText)
        {
            SourceCount++;
            m_generator.Logger.Information($" SourceAdded: {SourceCount}. {hintName}");
            m_context.AddSource(hintName, sourceText);
        }

        /// <inheritdoc/>
        public void ReportDiagnostic(Diagnostic diagnostic)
            => m_context.ReportDiagnostic(diagnostic);
    }
}