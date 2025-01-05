using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace SGF
{
    /// <summary>
    /// An abstraction on top of <see cref="SgfInitializationContext"/> that allow for some extra api
    /// and features not exposed in the public base.
    /// </summary>
    public interface ISgfInitializationContext
    {
        /// <summary>
        /// Gets access to the provider of <see cref="AdditionalText"/>
        /// </summary>
        IncrementalValuesProvider<AdditionalText> AdditionalTextsProvider { get; }

        /// <summary>
        /// Gets access to the provider for <see cref="AnalyzerConfigOptionsProvider"/>
        /// </summary>
        IncrementalValueProvider<AnalyzerConfigOptionsProvider> AnalyzerConfigOptionsProvider { get; }

        /// <summary>
        /// Gets access to the provider for <see cref="Compilation"/>
        /// </summary>
        IncrementalValueProvider<Compilation> CompilationProvider { get; }

        /// <summary>
        /// Gets access to the provider for <see cref="MetadataReference"/>
        /// </summary>
        IncrementalValuesProvider<MetadataReference> MetadataReferencesProvider { get; }

        /// <summary>
        /// Gets access to the provider for <see cref="ParseOptions"/>
        /// </summary>
        IncrementalValueProvider<ParseOptions> ParseOptionsProvider { get; }

        /// <summary>
        /// Gets access to the provider for <see cref="SyntaxProvider"/>
        /// </summary>
        SyntaxValueProvider SyntaxProvider { get; }

        /// <summary>
        /// Gets the original <see cref="IncrementalGeneratorInitializationContext"/> that was produced by
        /// Roslyn. This should only be used in cases where you are interacting with third party api that needs it. The
        /// issue is that SGF provides type loading an exception handling that MUST run before your code otherwise the generator 
        /// will crash. 
        /// </summary>
        IncrementalGeneratorInitializationContext OriginalContext { get; }

        /// <summary>
        /// Register a callback that produces source that is semantically 'invisible'; that is the added code has a runtime effect, 
        /// but adds no user visible types in completion or intellisense etc.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <param name="source">The source provider</param>
        /// <param name="action">The action that will be invoked</param>
        void RegisterImplementationSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);

        /// <summary>
        /// Register a callback that produces source that is semantically 'invisible'; that is the added code has a runtime effect, 
        /// but adds no user visible types in completion or intellisense etc.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <param name="sources">The source provider</param>
        /// <param name="action">The action that will be invoked</param>
        void RegisterImplementationSourceOutput<TSource>(IncrementalValuesProvider<TSource> sources, Action<SgfSourceProductionContext, TSource> action);

        /// <summary>
        /// Register a callback that produces source that will be added regardless of changes to any of the providers. This source does not have access to analyzer
        /// information so it must be constant. Unlike other callbacks types added here can be referenced as it's added to the compilation info. This is often used
        /// for creating <see cref="Attribute"/> that are used by the source generator.
        /// </summary>
        /// <param name="callback"></param>
        void RegisterPostInitializationOutput(Action<IncrementalGeneratorPostInitializationContext> callback);

        /// <summary>
        /// Registers a callback that produces source that effects compilation and intelliesense. This callback will effect IDE evaluation time so
        /// performance is critical.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <param name="source">The source to add</param>
        /// <param name="action">The action to be preformed</param>
        void RegisterSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action);

        /// <summary>
        /// Registers a callback that produces source that effects compilation and intelliesense. This callback will effect IDE evaluation time so
        /// performance is critical.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <param name="source">The source to add</param>
        /// <param name="action">The action to be preformed</param>
        void RegisterSourceOutput<TSource>(IncrementalValuesProvider<TSource> sourcess, Action<SgfSourceProductionContext, TSource> action);
    }
}