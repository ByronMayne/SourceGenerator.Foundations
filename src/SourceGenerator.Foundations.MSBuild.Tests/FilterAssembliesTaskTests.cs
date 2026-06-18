using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SGF.Shims;

namespace SourceGenerator.Foundations.MSBuild.Tests
{
    public class FilterAssembliesTaskTests
    {
        [Fact]
        public void ExcludesCodeAnalysisAssembliesByDefault()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
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
                BuildEngine = new BuildEngineShim(),
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

        [Fact]
        public void ExcludesIgnoredSystemAssemblies()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new("/tmp/mscorlib.dll"),
                    new("/tmp/System.dll"),
                    new("/tmp/netstandard.dll"),
                    new("/tmp/System.Runtime.CompilerServices.Unsafe.dll"),
                    new("/tmp/MyLibrary.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/MyLibrary.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void ExcludesNetStandardLibraryAssemblies()
        {
            string separator = System.IO.Path.DirectorySeparatorChar.ToString();
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new($"{separator}packages{separator}netstandard.library{separator}2.0.3{separator}System.Data.dll"),
                    new($"{separator}packages{separator}netstandard.library{separator}2.0.3{separator}System.Xml.dll"),
                    new($"{separator}packages{separator}mypackage{separator}1.0.0{separator}MyLibrary.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Contains("MyLibrary.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void HandlesEmptyAssemblyList()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = Array.Empty<ITaskItem>()
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Empty(task.FilteredAssemblies);
        }

        [Fact]
        public void SkipsNullOrWhitespaceAssemblyPaths()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new(""),
                    new("   "),
                    new("/tmp/ValidAssembly.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/ValidAssembly.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void IncludesMultipleValidAssemblies()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new("/tmp/Newtonsoft.Json.dll"),
                    new("/tmp/AutoMapper.dll"),
                    new("/tmp/Serilog.dll"),
                    new("/tmp/FluentValidation.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Equal(4, task.FilteredAssemblies.Length);
        }

        [Fact]
        public void ExcludesCodeAnalysisWithCaseInsensitiveMatch()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                EmbedCodeAnalysis = false,
                Assemblies = new TaskItem[]
                {
                    new("/tmp/microsoft.codeanalysis.dll"),
                    new("/tmp/MICROSOFT.CODEANALYSIS.CSHARP.dll"),
                    new("/tmp/Microsoft.CodeAnalysis.VisualBasic.dll"),
                    new("/tmp/MyAssembly.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/MyAssembly.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Theory]
        [InlineData("Microsoft.CSharp.dll")]
        [InlineData("System.Collections.Immutable.dll")]
        [InlineData("System.Reflection.Metadata.dll")]
        [InlineData("System.Memory.dll")]
        [InlineData("System.Buffers.dll")]
        public void ExcludesSpecificIgnoredAssembly(string assemblyName)
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new($"/tmp/{assemblyName}"),
                    new("/tmp/MyLibrary.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/MyLibrary.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void MixedScenarioWithMultipleFilterConditions()
        {
            string separator = System.IO.Path.DirectorySeparatorChar.ToString();
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                EmbedCodeAnalysis = false,
                Assemblies = new TaskItem[]
                {
                    new("/tmp/Microsoft.CodeAnalysis.dll"),
                    new("/tmp/System.dll"),
                    new($"{separator}packages{separator}netstandard.library{separator}2.0.3{separator}System.IO.dll"),
                    new("/tmp/mscorlib.dll"),
                    new(""),
                    new("/tmp/Newtonsoft.Json.dll"),
                    new("/tmp/MyCustomLibrary.dll"),
                    new("/tmp/System.Collections.Immutable.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Equal(2, task.FilteredAssemblies.Length);
            Assert.Contains(task.FilteredAssemblies, a => a.ItemSpec == "/tmp/Newtonsoft.Json.dll");
            Assert.Contains(task.FilteredAssemblies, a => a.ItemSpec == "/tmp/MyCustomLibrary.dll");
        }

        [Fact]
        public void PreservesTaskItemMetadata()
        {
            TaskItem assembly = new TaskItem("/tmp/MyLibrary.dll");
            assembly.SetMetadata("HintPath", "/some/path");
            assembly.SetMetadata("Private", "True");

            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new ITaskItem[] { assembly }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/some/path", task.FilteredAssemblies[0].GetMetadata("HintPath"));
            Assert.Equal("True", task.FilteredAssemblies[0].GetMetadata("Private"));
        }

        [Fact]
        public void ExecuteAlwaysReturnsTrue()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                Assemblies = new TaskItem[]
                {
                    new("/tmp/System.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
        }
    }
}
