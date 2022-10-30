using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SGF
{
    [Generator]
    public class FoundationsSourceGenerator : IIncrementalGenerator
    {
        private static readonly IHandlebars s_handlebars;
        static FoundationsSourceGenerator()
        {
            HandlebarsConfiguration configuration = new HandlebarsConfiguration()
            { };

            s_handlebars = Handlebars.Create(configuration);
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
            Compilation compilation = tuple.compilation;
            AnalyzerConfigOptionsProvider analyzerConfigOptions = tuple.analyzerConfigOptions;
            IDictionary<string, string> variables = new Dictionary<string, string>()
            {
                ["RootNamesapce"] = "SourceGenerator.Foundations"
            };

            Debugger.Launch();

            if (analyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace))
            {
                variables["RootNamespace"] = rootNamespace;
            }

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
                hintName = Path.ChangeExtension(hintName, ".cs");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string templates = reader.ReadToEnd();
                    var compiledTemplate = s_handlebars.Compile(templates);
                    string renderedTemplate = compiledTemplate(variables);
                    SourceText sourceText = SourceText.From(renderedTemplate, Encoding.UTF8);
                    context.AddSource(hintName, sourceText);
                }
            }
        }
    }
}
