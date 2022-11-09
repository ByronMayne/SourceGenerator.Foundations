#nullable enable
using SGF.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SGF
{
    public static class AssemblyResolver
    {
        private static readonly ISet<Assembly> s_assemblies;

        static AssemblyResolver()
        {
            s_assemblies = new HashSet<Assembly>();
            s_assemblies.Add(typeof(AssemblyResolver).Assembly);
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        [ModuleInitializer]
        internal static void InitializeResolver()
        {
            AssemblyName contractsAssembly = new AssemblyName("SourceGenerator.Foundations.Contracts");
            ResolveAssembly(contractsAssembly);
        }

        /// <summary>
        /// Attempts to resolve any assembly by looking for dependencies that are embedded directly
        /// in this dll.
        /// </summary>
        private static Assembly? OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName targetAssembly = new AssemblyName(args.Name);
            return ResolveAssembly(targetAssembly);
        }

        private static Assembly? ResolveAssembly(AssemblyName assemblyName)
        {
            string resourceName = $"{ResourceConfiguration.AssemblyResourcePrefix}{assemblyName.Name}.dll";

            foreach (Assembly assembly in s_assemblies)
            {
                ManifestResourceInfo resourceInfo = assembly.GetManifestResourceInfo(resourceName);
                if (resourceInfo != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        Assembly resolvedAssembly = Assembly.Load(data);

                        if (resolvedAssembly != null)
                        {
                            if (!s_assemblies.Contains(resolvedAssembly))
                            {
                                s_assemblies.Add(resolvedAssembly);
                            }
                            return resolvedAssembly;
                        }
                    }
                }
            }
            return null;
        }
    }
}