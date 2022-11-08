using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SGF.Diagnostics;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace SGF
{
    [Generator]
    public class FoundationsSourceGenerator : IIncrementalGenerator
    {
        public static ILogger Log { get; }

        private static readonly IHandlebars m_handlebars;
        static FoundationsSourceGenerator()
        {
            Log = DevelopmentEnviroment.Instance.GetLogger(typeof(FoundationsSourceGenerator).FullName);
            Log.LogInformation("Initializing");
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

        public void Initialize(IncrementalGeneratorInitializationContext context)
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
                AnalyzerConfigOptionsProvider analyzerConfigOptions = tuple.analyzerConfigOptions;
                IDictionary<string, string> variables = new Dictionary<string, string>();

                Assembly assembly = typeof(FoundationsSourceGenerator).Assembly;
                AssemblyName assemblyName = assembly.GetName();

                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    //SourceGenerator.Foundations.Templates.Diagnostics.ProcessExtensions.hbs
                    if (Path.GetExtension(resourceName) != ".hbs")
                    {
                        continue;
                    }

                    string hintName = resourceName.Replace($"{assemblyName.Name}.", "");
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
            catch(Exception exception)
            {
                Log.LogFatal(exception, "An unhandle exception of type {0} was thrown\n{1}", exception.GetType(), exception.ToString());
            }
        }
    }
}
