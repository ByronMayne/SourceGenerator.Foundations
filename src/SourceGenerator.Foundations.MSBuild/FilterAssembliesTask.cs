using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceGenerator.Foundations.MSBuild
{
    /// <summary>
    /// When choose which assemblies to be embedded we need to filter out quite a few
    /// to avoid them be embedded. For example any of the defalut ones in the GAC 'mscorlib', reference only 
    /// assemblies 'Microsoft.CSharp'. We have to analizes these inside a custom task because some of this
    /// information is only accessable from within C# code.
    /// </summary>
    public class FilterAssembliesTask : Task, ITask
    {
        private readonly string m_netStandardPattern;

        /// <summary>
        /// The list of assemblies that we can filter out 
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Gets the list of assemblies that have been excluded from the <see cref="Assemblies"/> list. 
        /// This is used to filter out assemblies that should not be embedded. This are based on the assembly name 
        /// not the full path
        /// </summary>
        [Required]
        public ITaskItem[] ExcludedAssemblyNames { get; set; }

        /// <summary>
        /// The filtered version of <see cref="Assemblies"/>
        /// </summary>
        [Output]
        public ITaskItem[] FilteredAssemblies { get; set; }

        public FilterAssembliesTask()
        {
            Assemblies = Array.Empty<ITaskItem>();
            ExcludedAssemblyNames = Array.Empty<ITaskItem>();
            FilteredAssemblies = Array.Empty<ITaskItem>();
            m_netStandardPattern = $"{Path.DirectorySeparatorChar}netstandard.library{Path.DirectorySeparatorChar}";
        }

        /// <inheritdoc cref="Task"/>
        public override bool Execute()
        {
            List<ITaskItem> filtered = new List<ITaskItem>(Assemblies.Length);

            HashSet<string> excludedAssemblyNames = new HashSet<string>(ExcludedAssemblyNames
                .Select(GetAssemblyName),
                StringComparer.OrdinalIgnoreCase);


            foreach (ITaskItem sourceAssembly in Assemblies)
            {
                string assemblyPath = sourceAssembly.ItemSpec;
                string assemblyName = GetAssemblyName(sourceAssembly);

                if (string.IsNullOrWhiteSpace(assemblyPath))
                {
                    continue;
                }

                if (assemblyPath.IndexOf(m_netStandardPattern, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    // Skipping netstandard.library assemblies as they are part of the .NET Standard framework and should not be embedded.
                    continue;
                }

                if (excludedAssemblyNames.Contains(assemblyName))
                {
                    Log.LogMessage(MessageImportance.Normal, "Skipping: {0}", assemblyPath);
                    continue;
                }

                filtered.Add(sourceAssembly);
                Log.LogMessage(MessageImportance.Normal, "Added: {0}", assemblyPath);
            }
            FilteredAssemblies = filtered.ToArray();
            return true;
        }

        private static string GetAssemblyName(ITaskItem taskItem)
        {
            string assemblyPath = taskItem.ItemSpec;
            string fileName = Path.GetFileName(assemblyPath);

            // Only strip known assembly file extensions. Names like
            // "Microsoft.CodeAnalysis.CSharp" are assembly identities, not file paths.
            if (fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileNameWithoutExtension(fileName);
            }

            return fileName;
        }
    }

}