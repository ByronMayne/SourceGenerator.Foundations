using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SGF;

public interface ISgfSourceProductionContext
{
    /// <summary>
    /// Gets the number of source files that were added
    /// </summary>
    int SourceCount { get; }
    /// <summary>
    /// A token that will be canceled when generation should stop
    /// </summary>
    CancellationToken CancellationToken { get; }
    /// <summary>
    /// Adds source code in the form of a <see cref="string"/> to the compilation.
    /// </summary>
    /// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator</param>
    /// <param name="source">The source code to add to the compilation</param>
    void AddSource(string hintName, string source);
    /// <summary>
    /// Adds a <see cref="SourceText"/> to the compilation
    /// </summary>
    /// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator</param>
    /// <param name="sourceText">The <see cref="SourceText"/> to add to the compilation</param>
    void AddSource(string hintName, SourceText sourceText);
    /// <summary>
    /// Adds a <see cref="Diagnostic"/> to the users compilation 
    /// </summary>
    /// <param name="diagnostic">The diagnostic that should be added to the compilation</param>
    /// <remarks>
    /// The severity of the diagnostic may cause the compilation to fail, depending on the <see cref="Compilation"/> settings.
    /// </remarks>
    void ReportDiagnostic(Diagnostic diagnostic);
}