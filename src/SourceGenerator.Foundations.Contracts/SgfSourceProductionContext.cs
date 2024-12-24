using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SGF.Diagnostics;
using System.Text;
using System.Threading;

namespace SGF
{
    /// <summary>
    /// Wrapper around a <see cref="SourceProductionContext"/> used to help capture errors and report logs
    /// </summary>
    public struct SgfSourceProductionContext : ISgfSourceProductionContext
    {
        private readonly ILogger m_logger;
        private readonly SourceProductionContext m_context;

        /// <inheritdoc/>
        public int SourceCount { get; private set; }

        /// <inheritdoc/>
        SourceProductionContext ISgfSourceProductionContext.OriginalContext => m_context;

        /// <summary>
        /// A token that will be canceled when generation should stop
        /// </summary>
        public CancellationToken CancellationToken => m_context.CancellationToken;

        internal SgfSourceProductionContext(SourceProductionContext context, ILogger logger)
        {
            SourceCount = 0;
            m_logger = logger;
            m_context = context;

        }

        /// <inheritdoc/>
        public void AddSource(string hintName, string source)
            => AddSource(hintName, SourceText.From(source, Encoding.UTF8));

        /// <inheritdoc/>
        public void AddSource(string hintName, SourceText sourceText)
        {
            SourceCount++;
            m_logger.Information($" SourceAdded: {SourceCount}. {hintName}");
            m_context.AddSource(hintName, sourceText);
        }

        /// <inheritdoc/>
        public void ReportDiagnostic(Diagnostic diagnostic)
            => m_context.ReportDiagnostic(diagnostic);
    }
}