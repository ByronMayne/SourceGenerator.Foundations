#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SGF.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace SGF.Generators
{
    [Generator]
    internal class ScriptInjectorGenerator : IncrementalGenerator
    {
        public ScriptInjectorGenerator() : base("ScriptInjector")
        { }

        protected override void OnInitialize(SgfInitializationContext context)
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
                Logger.Debug("Adding source files to {AssemblyName}", compilation.AssemblyName);
                AnalyzerConfigOptionsProvider analyzerConfigOptions = tuple.analyzerConfigOptions;

                Assembly assembly = typeof(ScriptInjectorGenerator).Assembly;
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
                    using Stream stream = assembly.GetManifestResourceStream(resourceName);
                    using StreamReader reader = new(stream);
                    string templates = reader.ReadToEnd();
                    SourceText sourceText = SourceText.From(templates, Encoding.UTF8);
                    context.AddSource(hintName, sourceText);
                    Logger.Debug("Added {FileName}", Path.GetFileName(hintName));
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception, "An unhandle exception of was thrown.");
            }
        }
    }
}
