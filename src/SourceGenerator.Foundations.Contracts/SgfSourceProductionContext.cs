using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using SGF.Diagnostics;
using System.Text;
using System.Threading;
using System.Reflection;
using System;

namespace SGF
{
    /// <summary>
    /// Wrapper around a <see cref="SourceProductionContext"/> used to help capture errors and report logs
    /// </summary>
    public struct SgfSourceProductionContext
    {
        private readonly ILogger m_logger;
        private readonly SourceProductionContext m_context;

        /// <summary>
        /// Gets the number of source files that were added
        /// </summary>
        public int SourceCount { get; private set; }


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

        /// <summary>
        /// Adds source code in the form of a <see cref="string"/> to the compilation.
        /// </summary>
        /// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator</param>
        /// <param name="source">The source code to add to the compilation</param>
        public void AddSource(string hintName, string source) => AddSource(hintName, SourceText.From(source, Encoding.UTF8));

        /// <summary>
        /// Adds a <see cref="SourceText"/> to the compilation
        /// </summary>
        /// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator</param>
        /// <param name="sourceText">The <see cref="SourceText"/> to add to the compilation</param>
        public void AddSource(string hintName, SourceText sourceText)
        {
            SourceCount++;
            m_logger.Information($" SourceAdded: {SourceCount}. {hintName}");
            m_context.AddSource(hintName, sourceText);
        }

        /// <summary>
        /// Adds a <see cref="Diagnostic"/> to the users compilation 
        /// </summary>
        /// <param name="diagnostic">The diagnostic that should be added to the compilation</param>
        /// <remarks>
        /// The severity of the diagnostic may cause the compilation to fail, depending on the <see cref="Compilation"/> settings.
        /// </remarks>
        public void ReportDiagnostic(Diagnostic diagnostic) => m_context.ReportDiagnostic(diagnostic);
    }
}