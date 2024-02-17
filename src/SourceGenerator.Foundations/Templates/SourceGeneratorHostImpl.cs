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

        public {{dataModel.ClassName}}Hoist() : base("{{dataModel.ClassName}}")
        {
            m_generator = null;        
        }

        /// <summary>
        /// Initializes the source generator to make it simpler to work with 
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // The expected arguments types for the generator being created 
            Type[] typeArguments = new Type[] 
            {
                typeof(IGeneratorEnvironment),
                typeof(ILogger),
            };
            
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type generatorType = typeof(global::{{dataModel.QualifedName}});
            ConstructorInfo? constructor = generatorType.GetConstructor(bindingFlags, null, typeArguments, Array.Empty<ParameterModifier>());

            if(constructor == null)
            {
                string argumentString = string.Join(", ", typeArguments.Select(t => t.Name));
                m_logger.Error($"Unable to create instance of {generatorType.FullName} as no constructor could be found that takes {argumentString}. The generator will not be run");
                return;
            }


            object[] constructorArguments = new  object[]
            {
                m_environment,
                m_logger
            };
            IncrementalGenerator generator = (global::{{dataModel.QualifedName}})constructor.Invoke(constructorArguments);
            m_generator = generator;
            try
            {
                SgfInitializationContext sgfContext = new(context, m_logger);

                generator.OnInitialize(sgfContext);
            }
            catch (Exception exception)
            {
                m_logger.Error(exception, $"Error! An unhandle exception was thrown while initializing the source generator '{Name}'.");
            }
        }

        protected override void Dispose()
        {
            base.Dispose();
            if(m_generator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
""", Encoding.UTF8);
}



