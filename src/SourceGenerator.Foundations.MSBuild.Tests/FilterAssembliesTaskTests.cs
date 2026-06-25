using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SGF.Shims;

namespace SourceGenerator.Foundations.MSBuild.Tests
{
    public class FilterAssembliesTaskTests
    {
        [Fact]
        public void ExcludesCodeAnalysisAssembliesByDefaultPattern()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                ExcludedAssemblyNames = new TaskItem[]
                {
                    new("Microsoft.CodeAnalysis*"),
                },
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
        public void ExcludesNamedAssemblyWithoutExtension()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                ExcludedAssemblyNames = new TaskItem[]
                {
                    new("MyDependency"),
                },
                Assemblies = new TaskItem[]
                {
                    new("/tmp/MyDependency.dll"),
                    new("/tmp/OtherDependency.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/OtherDependency.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void IncludesAssembliesWhenNoIgnoredPatternMatches()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                ExcludedAssemblyNames = new TaskItem[]
                {
                    new("Does.Not.Match*"),
                },
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
        public void ExcludesSpecificAssemblyFileName()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                ExcludedAssemblyNames = new TaskItem[]
                {
                    new("System.Collections.Immutable.dll"),
                },
                Assemblies = new TaskItem[]
                {
                    new("/tmp/System.Collections.Immutable.dll"),
                    new("/tmp/MyLibrary.dll"),
                }
            };

            bool result = task.Execute();

            Assert.True(result);
            Assert.Single(task.FilteredAssemblies);
            Assert.Equal("/tmp/MyLibrary.dll", task.FilteredAssemblies[0].ItemSpec);
        }

        [Fact]
        public void ExcludesDottedAssemblyNameWithoutExtension()
        {
            FilterAssembliesTask task = new FilterAssembliesTask
            {
                BuildEngine = new BuildEngineShim(),
                ExcludedAssemblyNames = new TaskItem[]
                {
                    new("Microsoft.CodeAnalysis.CSharp"),
                },
                Assemblies = new TaskItem[]
                {
                    new("/tmp/Microsoft.CodeAnalysis.CSharp.dll"),
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
    }
}
