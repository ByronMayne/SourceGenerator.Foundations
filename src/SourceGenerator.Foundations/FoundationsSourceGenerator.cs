using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SourceGenerator.Foundations
{
    [Generator]
    public class FoundationsSourceGenerator : IIncrementalGenerator
    {
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

            if(analyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace))
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
                using(StreamReader reader = new StreamReader(stream))
                {
                    string source = reader.ReadToEnd();
                    foreach(var pair in variables)
                    {
                        source = source.Replace("{{" + pair.Key + "}}", pair.Value);
                    }
                    SourceText sourceText = SourceText.From(source, Encoding.UTF8);
                    context.AddSource(hintName, sourceText);
                }
            }
        }
    }
}
