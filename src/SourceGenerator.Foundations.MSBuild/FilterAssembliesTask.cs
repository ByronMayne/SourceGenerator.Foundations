using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SourceGenerator.Foundations.MSBuild
{
    /// <summary>
    /// When choose which assemblies to be embedded we need to filter out quite a few
    /// to avoid them be embedded. For example any of the defalut ones in the GAC 'mscorlib', reference only 
    /// assemblies 'Microsoft.CSharp'. We have to analizes these inside a custom task because some of this
    /// information is only accessable from within C# code.
    /// </summary>
    public class FilterAssembliesTask : Task
    {
        private readonly string m_netStandardPatttern;
        private readonly ISet<string> m_ignoredAssemblies;

        /// <summary>
        /// The list of assemblies that we can filter out 
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// The filtered version of <see cref="Assemblies"/>
        /// </summary>
        [Output]
        public ITaskItem[] FilteredAssemblies { get; set; }


        public FilterAssembliesTask()
        {
            m_netStandardPatttern = $"{Path.DirectorySeparatorChar}netstandard.library{Path.DirectorySeparatorChar}";
            m_ignoredAssemblies = new HashSet<string>()
            {
                "Microsoft.CodeAnalysis.CSharp.dll",
                "Microsoft.CSharp.dll",
                "Microsoft.Win32.Primitives.dll",
                "mscorlib.dll",
                "netstandard.dll",
                "System.AppContext.dll",
                "System.Buffers.dll",
                "System.CodeDom.dll",
                "System.Collections.Immutable.dll",
                "System.Console.dll",
                "System.Diagnostics.dll",
                "System.dll",
                "System.Memory.dll",
                "System.Numerics.Vectors.dll",
                "System.ObjectModel.dll",
                "System.Reflection.Metadata.dll",
                "System.Runtime.CompilerServices.Unsafe.dll",
                "System.ServiceModel.dll",
                "System.Text.Encoding.CodePages.dll",
                "System.Threading.Tasks.Extensions.dll",
                "System.Transactions.dll",
                "System.ValueTuple.dll",
                "System.Web.dll",
                "System.Windows.dll",
                "System.Collections.dll",
            };
        }

        /// <inheritdoc cref="Task"/>
        public override bool Execute()
        {
            List<ITaskItem> filtered = new List<ITaskItem>(Assemblies.Length);

            foreach (ITaskItem assembly in Assemblies)
            {
                string assemblyPath = assembly.ItemSpec;
                if (string.IsNullOrWhiteSpace(assemblyPath))
                {
                    continue;
                }

                if (Include(assemblyPath))
                {
                    filtered.Add(assembly);
                    Log.LogMessage(MessageImportance.Normal, "Added: {0}", assemblyPath);
                }
                else
                {
                    Log.LogMessage(MessageImportance.Normal, "Skipping: {0}", assemblyPath);
                }
            }
            FilteredAssemblies = filtered.ToArray();
            return true;
        }

        /// <summary>
        /// Returns true if the assembly at the given path should be included otherwise false
        /// </summary>
        private bool Include(string assemblyPath)
        {
            foreach (string ignoredAssembly in m_ignoredAssemblies)
            {
                if (assemblyPath.IndexOf(m_netStandardPatttern, StringComparison.OrdinalIgnoreCase) > 0) return false;
                if (assemblyPath.EndsWith(ignoredAssembly))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
