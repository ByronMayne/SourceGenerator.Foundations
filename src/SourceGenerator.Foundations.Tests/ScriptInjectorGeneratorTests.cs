using SGF.Generators;
using Xunit.Abstractions;

namespace SourceGenerator.Foundations.Tests
{
    public class ScriptInjectorGeneratorTests : GeneratorTest
    {
        public ScriptInjectorGeneratorTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {}

        [Fact]
        public void AssemblyResolver_AddedToProject()
            => Compose<ScriptInjectorGenerator>("",
                    a => a.AnyTreeNamed("AssemblyResolver.generated.cs"));
        [Fact]
        public void ResourceConfiguration_AddedToProject()
            => Compose<ScriptInjectorGenerator>("",
                a => a.AnyTreeNamed("ResourceConfiguration.generated.cs"));

        [Fact]
        public void ModuleInitializerAttribute_AddedToProject()
        => Compose<ScriptInjectorGenerator>("",
                a => a.AnyTreeNamed("ModuleInitializerAttribute.generated.cs"));

        [Fact]
        public void IncrementalGenerator_AddedToProject()
            => Compose<ScriptInjectorGenerator>("",
                 a => a.AnyTreeNamed("IncrementalGenerator.generated.cs"));
    }
}
