using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SGF.Diagnostics;

namespace SGF;

public interface ISgfInitializationContext
{
    SyntaxValueProvider SyntaxProvider { get; }
    IncrementalValueProvider<Compilation> CompilationProvider { get; }
    IncrementalValueProvider<ParseOptions> ParseOptionsProvider { get; }
    IncrementalValuesProvider<AdditionalText> AdditionalTextsProvider { get; }
    IncrementalValueProvider<AnalyzerConfigOptionsProvider> AnalyzerConfigOptionsProvider { get; }
    IncrementalValuesProvider<MetadataReference> MetadataReferencesProvider { get; }
    /// <summary>
    /// Gets the underlying <see cref="ILogger"/> 
    /// </summary>
    /// <remarks>Provided as a convenience accessor to support advanced scenarios. Generally not needed but no harm if used.</remarks>
    ILogger Logger { get; }
    /// <summary>
    /// Gets the original <see cref="IncrementalGeneratorInitializationContext"/> 
    /// </summary>
    /// <remarks>Provided as a convenience accessor to support advanced scenarios. Generally should not be directly
    /// interacted with as it may produce side effects in the sgf generators. Use at your own risk.</remarks>
    IncrementalGeneratorInitializationContext Context { get; }
    void RegisterSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);
    void RegisterSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);
    void RegisterImplementationSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);
    void RegisterImplementationSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);
    void RegisterPostInitializationOutput(Action<IncrementalGeneratorPostInitializationContext> callback);
}