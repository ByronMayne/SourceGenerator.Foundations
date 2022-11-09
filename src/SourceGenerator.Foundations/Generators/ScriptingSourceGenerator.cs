using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using SGF.Configuration;
using SGF.Logging;

namespace SGF.Generators
{
    [Generator]
    public class ScriptingSourceGenerator : IncrementalGenerator
    {
        private readonly IHandlebars m_handlebars;

        public ScriptingSourceGenerator()
        {
            Log.LogInformation("Initalizing {0}", nameof(ScriptingSourceGenerator));
            try
            {
                m_handlebars = Handlebars.Create();
            }
            catch (Exception exception)
            {
                Log.LogError("An unhandle exception was thrown at start {0}", exception.ToString());
                throw;
            }
        }

        protected override void OnInitialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context
                .CompilationProvider
                .Combine(context.AnalyzerConfigOptionsProvider),
                CompilerSource);
        }

        private void CompilerSource(SourceProductionContext context, (Compilation compilation, AnalyzerConfigOptionsProvider analyzerConfigOptions) tuple)
        {
            try
            {
                Compilation compilation = tuple.compilation;
                Log.LogInformation($"Adding source files to {compilation.AssemblyName}");
                AnalyzerConfigOptionsProvider analyzerConfigOptions = tuple.analyzerConfigOptions;
                IDictionary<string, string> variables = new Dictionary<string, string>();

                Assembly assembly = typeof(ScriptingSourceGenerator).Assembly;
                AssemblyName assemblyName = assembly.GetName();

                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    if (!resourceName.StartsWith(ResourceConfiguration.ScriptPrefix))
                    {
                        continue;
                    }

                    string hintName = resourceName.Replace($"{ResourceConfiguration.ScriptPrefix}", "");
                    hintName = Path.ChangeExtension(hintName, ".generated.cs");
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string templates = reader.ReadToEnd();
                        var compiledTemplate = m_handlebars.Compile(templates);
                        string renderedTemplate = compiledTemplate(variables);
                        SourceText sourceText = SourceText.From(renderedTemplate, Encoding.UTF8);
                        context.AddSource(hintName, sourceText);
                        Log.LogInformation("Adding source file {0}", Path.GetFileName(hintName));
                    }
                }
                Log.LogInformation("Generation complete");
            }
            catch (Exception exception)
            {
                Log.LogFatal(exception, "An unhandle exception of type {0} was thrown\n{1}", exception.GetType(), exception.ToString());
            }
        }
    }
}
