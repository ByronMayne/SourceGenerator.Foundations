using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string m_netStandardPatttern;

        /// <summary>
        /// The list of assemblies that we can filter out 
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Assembly names or wildcard patterns that should not be embedded.
        /// </summary>
        public ITaskItem[] IgnoredAssemblies { get; set; }

        /// <summary>
        /// The filtered version of <see cref="Assemblies"/>
        /// </summary>
        [Output]
        public ITaskItem[] FilteredAssemblies { get; set; }

        public FilterAssembliesTask()
        {
			Assemblies = Array.Empty<ITaskItem>();
            IgnoredAssemblies = Array.Empty<ITaskItem>();
            FilteredAssemblies = Array.Empty<ITaskItem>();

			m_netStandardPatttern = $"{Path.DirectorySeparatorChar}netstandard.library{Path.DirectorySeparatorChar}";
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
            if (assemblyPath.IndexOf(m_netStandardPatttern, StringComparison.OrdinalIgnoreCase) > 0)
            {
                return false;
            }

            string assemblyFileName = Path.GetFileName(assemblyPath);
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

            foreach (ITaskItem ignoredAssembly in IgnoredAssemblies)
            {
                if (IsMatch(assemblyFileName, assemblyName, ignoredAssembly.ItemSpec))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsMatch(string assemblyFileName, string assemblyName, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return false;
            }

            if (pattern.IndexOfAny(new[] { '*', '?' }) >= 0)
            {
                return WildcardMatch(assemblyFileName, pattern)
                    || WildcardMatch(assemblyName, pattern);
            }

            if (Path.HasExtension(pattern))
            {
                return string.Equals(assemblyFileName, pattern, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(assemblyName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        private static bool WildcardMatch(string value, string pattern)
        {
            string regexPattern = "^"
                + Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".")
                + "$";

            return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}