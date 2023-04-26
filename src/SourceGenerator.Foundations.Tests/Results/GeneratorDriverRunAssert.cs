using Microsoft.CodeAnalysis;
using System.Text;

namespace SourceGenerator.Foundations.Tests.Results
{
    internal class GeneratorDriverRunAssert : ITreeAssert
    {
        private readonly GeneratorDriverRunResult m_result;

        public GeneratorDriverRunAssert(GeneratorDriverRunResult result)
        {
            m_result = result;
        }

        public void AnyTreeNamed(string name)
        {
            foreach (SyntaxTree tree in m_result.GeneratedTrees)
            {
                string filePath = tree.FilePath;
                string fileName = Path.GetFileName(filePath);
                if (string.Equals(fileName, name))
                {
                    return;
                }

            }

            StringBuilder stringBuilder = new();
            _ = stringBuilder.Append("No tree was found with name ")
                .Append(name)
                .Append(". The trees that were generated are called:")
                .AppendJoin("\n - ", m_result.GeneratedTrees.Select(t => Path.GetFileName(t.FilePath)));

            Assert.Fail(stringBuilder.ToString());
        }
    }
}
