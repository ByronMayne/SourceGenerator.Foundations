using Microsoft.Build.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SourceGenerator.Foundations.MSBuild
{
    /// <summary>
    /// When choose which assemblies to be embedded we need to filter out quite a few
    /// to avoid them be embedded. For example any of the defalut ones in the GAC 'mscorlib', reference only 
    /// assemblies 'Microsoft.CSharp'. We have to analizes these inside a custom task because some of this
    /// information is only accessable from within C# code.
    /// </summary>
    public class FilterAssembliesTask : ITask
    {
        private readonly ISet<string> m_ignoredNames;
        private readonly IList<string> m_ignoredPrefixes;

        /// <summary>
        /// The list of assemblies that we can filter out 
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <inheritdoc cref="ITask"/>
        public IBuildEngine BuildEngine { get; set; }

        /// <inheritdoc cref="ITask"/>
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// The filtered version of <see cref="Assemblies"/>
        /// </summary>
        [Output]
        public ITaskItem[] FilteredAssemblies { get; set; }


        public FilterAssembliesTask()
        {
            m_ignoredPrefixes = new List<string>()
            {
               "System.IO",
               "System.Collections",
               "System.ComponentModel",
               "System.Core",
               "System.Drawing",
               "System.Data",
               "System.Dynamic",
               "System.Globalization",
               "System.Linq",
               "System.Memory",
               "System.Net",
               "System.Numerics",
               "System.Reflection",
               "System.Resources",
               "System.Runtime",
               "System.Security",
               "System.Threading",
               "System.Text",
               "System.Xml",
               "System",
               "System.Buffers",
               "System.Console",
               "System.Buffers",
               "System.Buffers",
            };
            m_ignoredNames = new HashSet<string>()
            {
                "System.Collections",
                "System.Diagnostics",
                "System",
                "System.Console",
                "System.Buffers",
                "System.Buffers",
                "System.Web",
                "System.Windows",
                "System.ValueTuple",
                "Microsoft.Win32.Primitives",
                "netstandard",
                "System.AppContext",
                "mscorlib",
                "System.ObjectModel",
                "System.ServiceModel*",
                "System.Transactions",
                "Microsoft.CodeAnalysis*",
                "Microsoft.CSharp",
            };
        }

        public bool Execute()
        {
            List<ITaskItem> filtered = new List<ITaskItem>(Assemblies.Length);

            foreach (ITaskItem assembly in Assemblies)
            {
                string assemblyPath = assembly.ItemSpec;
                if (string.IsNullOrWhiteSpace(assemblyPath)) continue;
                if (assemblyPath.Contains("\\netstandard.library\\")) continue; // ignore built in 

                filtered.Add(assembly);

            }
            FilteredAssemblies = filtered.ToArray();
            return true;
        }

    }
}
