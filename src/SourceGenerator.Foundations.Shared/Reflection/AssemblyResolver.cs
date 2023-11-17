#nullable enable
using SGF.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SGF.Reflection
{
    internal static class AssemblyResolver
    {
        private static readonly ConcurrentBag<Assembly> s_assembliesWithResources;
        private static readonly Dictionary<AssemblyName, Assembly> s_loadedAssemblies;

        static AssemblyResolver()
        {
            s_assembliesWithResources = new ConcurrentBag<Assembly>();
            s_loadedAssemblies = new Dictionary<AssemblyName, Assembly>(new AssemblyNameComparer());
        }

        internal static void Initialize()
        {
            // The assembly resolvers get added to multiple source generators 
            // so what we do here is only allow the first one defined to allow 
            // itself to be a resolver. Since this could lead to cases where two resolvers
            // exists and provide two different instances of the same assembly.
            const string RESOLVER_ATTACHED_KEY = "SGF_ASSEMBLY_RESOLVER_IS_ATTACHED";
            AppDomain currentDomain = AppDomain.CurrentDomain;
            object? rawValue = currentDomain.GetData(RESOLVER_ATTACHED_KEY);

            if (rawValue == null || (rawValue is bool isAttached && !isAttached))
            {
                currentDomain.SetData(RESOLVER_ATTACHED_KEY, true);
                currentDomain.AssemblyLoad += OnAssemblyLoaded;
                currentDomain.AssemblyResolve += ResolveMissingAssembly;

                foreach (Assembly assembly in currentDomain.GetAssemblies())
                {
                    AddAssembly(assembly);
                }
            }
        }

        /// <summary>
        /// Raised whenever our app domain loads a new assembly
        /// </summary>
        /// <param name="sender">THe thing that raised the event</param>
        /// <param name="args">The parameters</param>
        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            AddAssembly(args.LoadedAssembly);
        }

        /// <summary>
        /// Adds an assembly to the veriuos collections used to keep track of loaded items
        /// </summary>
        private static void AddAssembly(Assembly assembly)
        {
            AssemblyName assemblyName = assembly.GetName();

            if (s_loadedAssemblies.ContainsKey(assemblyName))
            {
                return;
            }
            s_loadedAssemblies.Add(assemblyName, assembly);

            if (assembly.IsDynamic) return;

            string[] resources = assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(ResourceConfiguration.AssemblyResourcePrefix))
                .ToArray();

            if (resources.Length == 0) return;

            foreach (string resource in resources)
            {
                Console.WriteLine($"Extracting: {resource}");
                if (TryExtractingAssembly(assembly, resource, out Assembly? loadedAssembly))
                {
                    AddAssembly(loadedAssembly!);
                }
            }
        }

        /// <summary>
        /// Attempts to resolve any assembly by looking for dependencies that are embedded directly
        /// in this dll.
        /// </summary>
        private static Assembly? ResolveMissingAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new(args.Name);

            if (s_loadedAssemblies.TryGetValue(assemblyName, out Assembly assembly))
            {
                return assembly;
            }

            foreach (Assembly loadedAssembly in s_assembliesWithResources)
            {
                string resourceName = $"{ResourceConfiguration.AssemblyResourcePrefix}{assemblyName.Name}.dll";
                if (TryExtractingAssembly(loadedAssembly, resourceName, out Assembly? extractedAssembly))
                {
                    AddAssembly(extractedAssembly!);
                    return extractedAssembly!;
                };
            }

            return null;
        }


        /// <summary>
        /// Attempts to load an assembly that is contained within aonther assembly as a resource
        /// </summary>
        /// <param name="assembly">The assembly that should contain the resource</param>
        /// <param name="resourceName">The expected name of the reosurce</param>
        /// <param name="loadedAssembly">The assembly if it was loaded</param>
        /// <returns>True if the assembly could be loaded otherwise false</returns>
        private static bool TryExtractingAssembly(Assembly assembly, string resourceName, out Assembly? loadedAssembly)
        {
            loadedAssembly = null;
            if (TryGetResourceBytes(assembly, resourceName, out byte[]? assemblyBytes))
            {
                loadedAssembly = TryGetResourceBytes(assembly, Path.ChangeExtension(resourceName, ".pdb"), out byte[]? symbolBytes)
                    ? Assembly.Load(assemblyBytes, symbolBytes)
                    : Assembly.Load(assemblyBytes);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to read bytes from a resource and returns back if it's successful or not
        /// </summary>
        /// <param name="assembly">The assembly to pull the resource from</param>
        /// <param name="resourceName">The name of the resource</param>
        /// <param name="bytes">The bytes[] if the resource could be found</param>
        /// <returns>True if the resource was found otherwise false</returns>
        private static bool TryGetResourceBytes(Assembly assembly, string resourceName, out byte[]? bytes)
        {
            bytes = null;
            ManifestResourceInfo resourceInfo = assembly.GetManifestResourceInfo(resourceName);
            if (resourceInfo == null)
            {
                return false;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                bytes = new byte[stream.Length];
                _ = stream.Read(bytes, 0, bytes.Length);
            }

            return true;
        }
    }
}