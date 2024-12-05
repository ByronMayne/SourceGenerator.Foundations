using Microsoft.CodeAnalysis.Text;
using SGF.Models;
using System.Text;

namespace SGF.Templates;

internal static class SourceGeneratorHostImpl
{
    public static SourceText RenderTemplate(SourceGeneratorDataModel dataModel) => SourceText.From($$"""
#nullable enable
using SGF;
using SGF.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Text;

namespace {{dataModel.Namespace}}
{
    [global::Microsoft.CodeAnalysis.GeneratorAttribute]
    internal class {{dataModel.ClassName}}Hoist : SourceGeneratorHoist, IIncrementalGenerator
    {
        // Has to be untyped otherwise it will try to resolve at startup
        private Lazy<object?> m_lazyGenerator;

        /// <summary>
        /// Creates a new generator host that will create an instance of {{dataModel.ClassName}} at runtime.
        /// </summary>
        public {{dataModel.ClassName}}Hoist() : base()
        {
            m_lazyGenerator = new Lazy<object?>(CreateInstance);    
        }

        /// <summary>
        /// Creates a new generator host that will reuse an existing instance of {{dataModel.ClassName}} instead of creating one dynamically.
        /// This function would only ever be called from unit tests.
        /// </summary>
        public {{dataModel.ClassName}}Hoist({{dataModel.ClassName}} generator): base()
        {
            m_lazyGenerator = new Lazy<object?>(() => generator);    
        }

        /// <summary>
        /// Initializes the source generator to make it simpler to work with 
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalGenerator? generator = m_lazyGenerator.Value as IncrementalGenerator;

            if(generator == null)
            {
                return;
            }

            ILogger logger = generator.Logger;
            try
            {
                SgfInitializationContext sgfContext = new(context, logger);

                generator.OnInitialize(sgfContext);
            }
            catch (Exception exception)
            {
                logger.Error(exception, $"Error! An unhandled exception was thrown while initializing the source generator '{{dataModel.QualifiedName}}'.");
            }
        }

        private object? CreateInstance()
        {
            // The expected arguments types for the generator being created 
            Type[] typeArguments = new Type[] { };
            
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type generatorType = typeof(global::{{dataModel.QualifiedName}});
            ConstructorInfo? constructor = generatorType.GetConstructor(bindingFlags, null, typeArguments, Array.Empty<ParameterModifier>());

            if(constructor == null)
            {
                return null;
            }

            object[] constructorArguments = new  object[]{};
            IncrementalGenerator generator = (global::{{dataModel.QualifiedName}})constructor.Invoke(constructorArguments);

            return generator;
        }

        public void Dispose()
        {
            if(m_lazyGenerator.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
""", Encoding.UTF8);
}



