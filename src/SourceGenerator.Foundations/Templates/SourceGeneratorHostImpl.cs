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
        private object? m_generator;

        public {{dataModel.ClassName}}Hoist() : base()
        {
            m_generator = null;        
        }

        /// <summary>
        /// Initializes the source generator to make it simpler to work with 
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // The expected arguments types for the generator being created 
            Type[] typeArguments = new Type[] { };
            
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type generatorType = typeof(global::{{dataModel.QualifedName}});
            ConstructorInfo? constructor = generatorType.GetConstructor(bindingFlags, null, typeArguments, Array.Empty<ParameterModifier>());

            if(constructor == null)
            {
                return;
            }


            object[] constructorArguments = new  object[]{};
            IncrementalGenerator generator = (global::{{dataModel.QualifedName}})constructor.Invoke(constructorArguments);
            ILogger logger = generator.Logger;

            m_generator = generator;
            try
            {
                SgfInitializationContext sgfContext = new(context, logger);

                generator.OnInitialize(sgfContext);
            }
            catch (Exception exception)
            {
                logger.Error(exception, $"Error! An unhandle exception was thrown while initializing the source generator '{{dataModel.QualifedName}}'.");
            }
        }

        public void Dispose()
        {
            if(m_generator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
""", Encoding.UTF8);
}



