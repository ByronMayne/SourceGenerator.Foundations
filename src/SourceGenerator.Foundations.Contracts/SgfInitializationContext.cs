using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SGF.Diagnostics;
using System;
using System.Reflection;

namespace SGF
{

    /// <summary>
    /// Middleware wrapper around a <see cref="IncrementalGeneratorInitializationContext"/> to allow for
    /// wrapping with exception handling and provide a better user experience 
    /// </summary>
    public readonly struct SgfInitializationContext : ISgfInitializationContext
    {
        private readonly ILogger m_logger;
        private readonly IncrementalGeneratorInitializationContext m_context;

        /// <inheritdoc/>
        public SyntaxValueProvider SyntaxProvider => m_context.SyntaxProvider;

        /// <inheritdoc/>
        public IncrementalValueProvider<Compilation> CompilationProvider => m_context.CompilationProvider;

        /// <inheritdoc/>
        public IncrementalValueProvider<ParseOptions> ParseOptionsProvider => m_context.ParseOptionsProvider;

        /// <inheritdoc  />
        public IncrementalValuesProvider<AdditionalText> AdditionalTextsProvider => m_context.AdditionalTextsProvider;

        /// <inheritdoc/>
        public IncrementalValueProvider<AnalyzerConfigOptionsProvider> AnalyzerConfigOptionsProvider => m_context.AnalyzerConfigOptionsProvider;

        /// <inheritdoc/>
        public IncrementalValuesProvider<MetadataReference> MetadataReferencesProvider => m_context.MetadataReferencesProvider;

        /// <inheritdoc/>
        IncrementalGeneratorInitializationContext ISgfInitializationContext.OriginalContext => m_context;

        public SgfInitializationContext(
            IncrementalGeneratorInitializationContext context,
            ILogger logger)
        {
            m_logger = logger;
            m_context = context;
        }

        ///  <inheritdoc/>
        public void RegisterSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action)
        {
            ILogger logger = m_logger;

            void wrappedAction(SourceProductionContext context, TSource source)
            {
                try
                {
                    action(new(context, logger), source);
                }
                catch (Exception exception)
                {
                    LogException(logger, exception, action.Method);
                }
            }
            m_context.RegisterSourceOutput(source, wrappedAction);
        }

        ///  <inheritdoc/>
        public void RegisterSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action)
        {
            ILogger logger = m_logger;
            void wrappedAction(SourceProductionContext context, TSource source)
            {
                try
                {
                    action(new(context, logger), source);
                }
                catch (Exception exception)
                {
                    LogException(logger, exception, action.Method);
                }
            }
            m_context.RegisterSourceOutput(source, wrappedAction);
        }

        ///  <inheritdoc/>
        public void RegisterImplementationSourceOutput<TSource>(IncrementalValueProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action)
        {
            ILogger logger = m_logger;
            void wrappedAction(SourceProductionContext context, TSource source)
            {
                try
                {
                    SgfSourceProductionContext sgfContext = new SgfSourceProductionContext(context, logger);
                    action(sgfContext, source);
                    logger.Information($" SourceFiles: {sgfContext.SourceCount}");
                }
                catch (Exception exception)
                {
                    LogException(logger, exception, action.Method);
                }
            }
            m_context.RegisterImplementationSourceOutput(source, wrappedAction);
        }

        ///  <inheritdoc/>
        public void RegisterImplementationSourceOutput<TSource>(IncrementalValuesProvider<TSource> source, Action<SgfSourceProductionContext, TSource> action)
        {
            ILogger logger = m_logger;

            void wrappedAction(SourceProductionContext context, TSource source)
            {
                try
                {
                    action(new(context, logger), source);
                }
                catch (Exception exception)
                {
                    LogException(logger, exception, action.Method);
                }
            }
            m_context.RegisterImplementationSourceOutput(source, wrappedAction);
        }

        ///  <inheritdoc/>
        public void RegisterPostInitializationOutput(Action<IncrementalGeneratorPostInitializationContext> callback)
        {
            ILogger logger = m_logger;
            void wrappedCallback(IncrementalGeneratorPostInitializationContext context)
            {
                try
                {
                    callback(context);
                }
                catch (Exception exception)
                {
                    LogException(logger, exception, callback.Method);
                }
            }
            m_context.RegisterPostInitializationOutput(wrappedCallback);
        }

        /// <summary>
        /// Logs an exception to the lagger to be presented in the IDE.
        /// </summary>
        private static void LogException(ILogger logger, Exception exception, MethodInfo actionInfo)
        {
            string methodName = actionInfo.Name;
            string className = actionInfo.DeclaringType.FullName;
            logger.Error(exception, $"An {exception.GetType().Name} exception was thrown while invoking {className}.{methodName}");
        }
    }
}
