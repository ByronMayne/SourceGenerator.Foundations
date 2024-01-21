using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SGF.Diagnostics;
using System;
using System.Reflection;

namespace SGF
{
    /// <summary>
    /// Middleware wrapper around a <see cref="IncrementalGeneratorInitializationContext"/> to allow for
    /// wraping with exception handling and provide a better user expereince 
    /// </summary>
    internal readonly struct SgfInitializationContext
    {
        private readonly ILogger m_logger;
        private readonly IncrementalGeneratorInitializationContext m_context;

        public SyntaxValueProvider SyntaxProvider => m_context.SyntaxProvider;
        public IncrementalValueProvider<Compilation> CompilationProvider => m_context.CompilationProvider;
        public IncrementalValueProvider<ParseOptions> ParseOptionsProvider => m_context.ParseOptionsProvider;
        public IncrementalValuesProvider<AdditionalText> AdditionalTextsProvider => m_context.AdditionalTextsProvider;
        public IncrementalValueProvider<AnalyzerConfigOptionsProvider> AnalyzerConfigOptionsProvider => m_context.AnalyzerConfigOptionsProvider;
        public IncrementalValuesProvider<MetadataReference> MetadataReferencesProvider => m_context.MetadataReferencesProvider;

        public SgfInitializationContext(
            IncrementalGeneratorInitializationContext context,
            ILogger logger)
        {
            m_logger = logger;
            m_context = context;
        }

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

        private static void LogException(ILogger logger, Exception exception, MethodInfo actionInfo)
        {
            string methodName = actionInfo.Name;
            string className = actionInfo.DeclaringType.FullName;
            logger.Error(exception, $"An {exception.GetType().Name} exception was thrown while invoking {className}.{methodName}");
        }
    }
}
