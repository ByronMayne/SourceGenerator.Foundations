using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SourceGenerator.Foundations.MSBuild;

namespace ConsoleApp.SourceGenerator.Tests
{
    public class FilterAssembliesTaskTests
    {
        [Fact]
        public void ExcludesCodeAnalysisAssembliesByDefault()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new TestBuildEngine(),
                Assemblies = new TaskItem[]
                {
                    new("/tmp/Microsoft.CodeAnalysis.dll"),
                    new("/tmp/Microsoft.CodeAnalysis.CSharp.dll"),
                    new("/tmp/Newtonsoft.Json.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/Newtonsoft.Json.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void IncludesCodeAnalysisAssembliesWhenEnabled()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new TestBuildEngine(),
                EmbedCodeAnalysis = true,
                Assemblies = new TaskItem[]
                {
                    new("/tmp/Microsoft.CodeAnalysis.dll"),
                    new("/tmp/Microsoft.CodeAnalysis.CSharp.dll"),
                    new("/tmp/MyDependency.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Equal(3, task.FilteredAssemblies.Length);
        }

        private sealed class TestBuildEngine : IBuildEngine
        {
            public bool ContinueOnError => false;
            public int LineNumberOfTaskNode => 0;
            public int ColumnNumberOfTaskNode => 0;
            public string ProjectFileOfTaskNode => string.Empty;
            public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
            {
                return true;
            }

            public void LogCustomEvent(CustomBuildEventArgs e) { }
            public void LogErrorEvent(BuildErrorEventArgs e) { }
            public void LogMessageEvent(BuildMessageEventArgs e) { }
            public void LogWarningEvent(BuildWarningEventArgs e) { }
        }
    }
}
