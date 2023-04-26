using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator.Foundations.Tests.Results;
using Xunit.Abstractions;

namespace SourceGenerator.Foundations.Tests
{

    public abstract class GeneratorTest : BaseTest
    {
  

        protected GeneratorTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }



        protected void Compose<T>(string source,
            params Action<ITreeAssert>[] treeAsserts) where T : IIncrementalGenerator, new()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: $"{TestMethodName}",
                syntaxTrees: new[] { syntaxTree });

            T incrementalGenerator = new();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(incrementalGenerator);

            driver = driver.RunGenerators(compilation);


            GeneratorDriverRunResult result = driver.GetRunResult();
            GeneratorDriverRunAssert assertResult = new GeneratorDriverRunAssert(result);

            foreach(Action<ITreeAssert> assert in treeAsserts)
            {
                assert(assertResult);
            }

        }
    }
}
