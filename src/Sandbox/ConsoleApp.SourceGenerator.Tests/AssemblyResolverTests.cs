using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ConsoleApp.SourceGenerator.Tests
{
    /// <summary>
    /// The hoist's assembly resolver is process-wide, and a compiler server hosts the analyzers of every project
    /// it ever built. So the resolver meets assemblies it was not built with: another build of the same generator,
    /// or another generator that embeds a different version of the same dependency.
    /// </summary>
    public class AssemblyResolverTests
    {
        public AssemblyResolverTests()
        {
            // The resolver attaches in the hoist's module initializer.
            RuntimeHelpers.RunModuleConstructor(typeof(ConsoleAppSourceGeneratorHoist).Module.ModuleHandle);
        }

        [Fact]
        public void Embedded_Assembly_Resolves_When_An_Assembly_With_The_Same_Name_Was_Loaded_First()
        {
            // An older build of the generator, without embedded assemblies, takes the simple name first.
            LoadInOwnContext(Emit("ShadowedGenerator", "1.0.0.0"));
            byte[] dependency = Emit("ShadowedDependency", "2.0.0.0");
            LoadInOwnContext(Emit("ShadowedGenerator", "2.0.0.0", ("SGF.Assembly::ShadowedDependency.dll", dependency)));

            Assembly resolved = Assembly.Load(new AssemblyName("ShadowedDependency, Version=2.0.0.0"));

            Assert.Equal(new Version(2, 0, 0, 0), resolved.GetName().Version);
        }

        [Fact]
        public void Embedded_Assembly_Resolves_In_The_Requested_Version()
        {
            byte[] older = Emit("SharedDependency", "1.0.0.0");
            byte[] newer = Emit("SharedDependency", "2.0.0.0");
            LoadInOwnContext(Emit("OlderGenerator", "1.0.0.0", ("SGF.Assembly::SharedDependency.dll", older)));
            LoadInOwnContext(Emit("NewerGenerator", "1.0.0.0", ("SGF.Assembly::SharedDependency.dll", newer)));

            Assembly resolved = Assembly.Load(new AssemblyName("SharedDependency, Version=2.0.0.0"));

            Assert.Equal(new Version(2, 0, 0, 0), resolved.GetName().Version);
        }

        [Fact]
        public void Embedded_Assembly_Resolves_For_A_Request_Without_A_Version()
        {
            byte[] dependency = Emit("UnversionedDependency", "3.0.0.0");
            LoadInOwnContext(Emit("UnversionedGenerator", "1.0.0.0", ("SGF.Assembly::UnversionedDependency.dll", dependency)));

            Assembly resolved = Assembly.Load(new AssemblyName("UnversionedDependency"));

            Assert.Equal(new Version(3, 0, 0, 0), resolved.GetName().Version);
        }

        [Fact]
        public void Request_For_An_Older_Version_Resolves_To_The_Newer_One_Loaded()
        {
            byte[] dependency = Emit("UnifiedDependency", "2.0.0.0");
            LoadInOwnContext(Emit("UnifyingGenerator", "1.0.0.0", ("SGF.Assembly::UnifiedDependency.dll", dependency)));

            Assembly resolved = Assembly.Load(new AssemblyName("UnifiedDependency, Version=1.0.0.0"));

            Assert.Equal(new Version(2, 0, 0, 0), resolved.GetName().Version);
        }

        [Fact]
        public void Request_For_A_Newer_Version_Does_Not_Resolve_To_An_Older_One()
        {
            byte[] dependency = Emit("OutdatedDependency", "1.0.0.0");
            LoadInOwnContext(Emit("OutdatedGenerator", "1.0.0.0", ("SGF.Assembly::OutdatedDependency.dll", dependency)));

            Assert.Throws<FileNotFoundException>(() => Assembly.Load(new AssemblyName("OutdatedDependency, Version=2.0.0.0")));
        }

        private static byte[] Emit(string name, string version, params (string Name, byte[] Content)[] resources)
        {
            CSharpCompilation compilation = CSharpCompilation.Create(
                name,
                [CSharpSyntaxTree.ParseText($"[assembly: System.Reflection.AssemblyVersion(\"{version}\")]")],
                [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream stream = new();
            EmitResult result = compilation.Emit(
                stream,
                manifestResources: resources.Select(r => new ResourceDescription(r.Name, () => new MemoryStream(r.Content), isPublic: true)));
            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            return stream.ToArray();
        }

        // Roslyn loads each analyzer directory in a context of its own; the resolver sees them all through the AppDomain.
        private static void LoadInOwnContext(byte[] image)
            => new AssemblyLoadContext(name: null).LoadFromStream(new MemoryStream(image));
    }
}
