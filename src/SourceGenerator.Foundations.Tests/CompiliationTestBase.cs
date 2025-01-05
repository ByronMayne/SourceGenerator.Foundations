using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace SGF
{
    /// <summary>
    /// There is an attribute called `ExtendedAnalyzerRules` which are applied to source generators. Since
    /// SGF injects code into their projects we want to make sure we don't add warnings or errors.
    /// </summary>
    public class CompilationTestBase
    {
        public SgfAnalyzerConfigOptions AnalyzerOptions { get; }

        /// <summary>
        /// Gets the name of the currently running test method 
        /// </summary>
        public string TestMethodName { get; }

        private readonly List<MetadataReference> m_references;
        private readonly List<DiagnosticAnalyzer> m_analyzers;
        protected readonly List<IIncrementalGenerator> m_incrementalGenerators;

        protected CompilationTestBase(ITestOutputHelper outputHelper)
        {
            m_analyzers = new List<DiagnosticAnalyzer>();
            m_references = new List<MetadataReference>();
            m_incrementalGenerators = new List<IIncrementalGenerator>();

            AnalyzerOptions = new SgfAnalyzerConfigOptions();
            TestMethodName = "Unknown";
            Type type = outputHelper.GetType();
            FieldInfo? testField = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            ITest? test = testField?.GetValue(outputHelper) as ITest;
            if (test != null)
            {
                TestMethodName = test.TestCase.TestMethod.ToString()!;
            }

            AddAssemblyReference("System.Runtime");
            AddAssemblyReference("netstandard");
            AddAssemblyReference("System.Console");
            AddAssemblyReference("Microsoft.CodeAnalysis");
            AddAssemblyReference("System.Linq");
            AddMetadataReference<object>();
            AddMetadataReference<IncrementalGenerator>();
        }



        protected async Task ComposeAsync(
            string[]? source = null,
            Action<ImmutableArray<Diagnostic>>[]? assertDiagnostics = null,
            Action<Compilation>[]? assertCompilation = null)
        {

            source ??= [];

            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            CompilationWithAnalyzersOptions analyzerOptions = new CompilationWithAnalyzersOptions(
                new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, new SgfAnalyzerConfigOptionsProvider(AnalyzerOptions)),
                null, true, false);

            // Create a Roslyn compilation for the syntax tree.
            Compilation compilation = CSharpCompilation.Create(TestMethodName)
                .AddSyntaxTrees(source.Select(t => ParseSyntaxTree(t)).ToArray())
                .WithOptions(compilationOptions)
                .AddReferences(m_references);

            // Create an instance of our EnumGenerator incremental source generator
            HoistSourceGenerator generator = new HoistSourceGenerator();

            // The GeneratorDriver is used to run our generator against a compilation
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the source generator!
            driver = driver.RunGenerators(compilation)
                .RunGeneratorsAndUpdateCompilation(compilation, out compilation, out var _);

            CompilationWithAnalyzers analysis = compilation.WithAnalyzers(m_analyzers.ToImmutableArray(), analyzerOptions);

            ImmutableArray<Diagnostic> diagnostics = await analysis.GetAllDiagnosticsAsync();
            if (assertDiagnostics is not null)
            {
                foreach (Action<ImmutableArray<Diagnostic>> assert in assertDiagnostics)
                {
                    assert(diagnostics);
                }
            }

            if (assertCompilation is not null)
            {
                foreach (Action<Compilation> assert in assertCompilation)
                {
                    assert(compilation);
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="DiagnosticAnalyzer"/> that will be eveluated during compliation.
        /// </summary>
        protected void AddAnalyzer<T>() where T : DiagnosticAnalyzer, new()
            => m_analyzers.Add(new T());

        /// <summary>
        /// Adds a new <see cref="DiagnosticAnalyzer"/> that will be eveluated during compliation.
        /// </summary>
        protected void AddAnalyzer<T>(T instance) where T : DiagnosticAnalyzer
            => m_analyzers.Add(instance);

        /// <summary>
        /// Adds a new <see cref="IIncrementalGenerator"/> that will be executed during compliation.
        /// </summary>
        protected void AddGenerator<T>() where T : IIncrementalGenerator, new()
            => m_incrementalGenerators.Add(new T());

        /// <summary>
        /// Adds a new <see cref="IIncrementalGenerator"/> that will be executed during compliation.
        /// </summary>
        protected void AddGenerator<T>(T instance) where T : IIncrementalGenerator
            => m_incrementalGenerators.Add(instance);

        /// <summary>
        /// Adds a new <see cref="MetadataReference"/> for the given assembly that the type belongs in
        /// </summary>
        protected void AddMetadataReference<T>()
            => m_references.Add(MetadataReference.CreateFromFile(typeof(T).Assembly.Location));

        protected void AddAssemblyReference(string assemblyName)
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly is not null)
            {
                m_references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        }

        protected static SyntaxTree ParseSyntaxTree(string source, string fileName = "TestClass.cs")
        {
            SourceText sourceText = SourceText.From(source, Encoding.UTF8);
            CSharpParseOptions parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, path: fileName);
            return syntaxTree;
        }
    }
}