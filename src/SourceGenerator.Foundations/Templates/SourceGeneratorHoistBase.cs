
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace SGF.Templates;

public static class SourceGeneratorHoistBase
{
    public static SourceText RenderTemplate(string @namespace) => SourceText.From($$"""
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SGF.Environments;
using SGF.Diagnostics;
using SGF.Diagnostics.Sinks;
using System.Runtime.CompilerServices;

namespace {{@namespace}}
{
    /// <summary>
    /// Provides logic for hooking into the app domain to resolve assemblies as well
    /// as capture exceptions and handle shutdown events 
    /// </summary>
    internal abstract class SourceGeneratorHoist
    {
        private static bool s_isInitialized;
        private static readonly List<Assembly> s_assembliesWithResources;
        private static readonly Dictionary<AssemblyName, Assembly> s_loadedAssemblies;

        static SourceGeneratorHoist()
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            if(bool.TryParse(System.Environment.GetEnvironmentVariable("SGF_DEBUGGER_LAUNCH"), out bool launchDebugger)
                && launchDebugger)
            {
                System.Diagnostics.Debugger.Launch();
            }
#pragma warning restore RS1035 // Do not use APIs banned for analyzers


            s_assembliesWithResources = new List<Assembly>();
            s_loadedAssemblies = new Dictionary<AssemblyName, Assembly>(new AssemblyNameComparer());
            Initialize();
        }

        /// <summary>
        /// Used to initialize the source generators.
        /// </summary>
        [ModuleInitializer]
        internal static void Initialize()
        {
            if(s_isInitialized)
            {
                return;
            }
            s_isInitialized = true;

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
        private static void OnAssemblyLoaded(object? sender, AssemblyLoadEventArgs args)
        {
            AddAssembly(args.LoadedAssembly);
        }

        /// <summary>
        /// Adds an assembly to the various collections used to keep track of loaded items
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
                .Where(r => r.StartsWith("SGF.Assembly::"))
                .ToArray();

            if (resources.Length == 0) return;

            foreach (string resource in resources)
            {

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                System.Console.WriteLine($"Extracting {resource} assembly from {assemblyName.Name}'s resources.");
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
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
        private static Assembly? ResolveMissingAssembly(object? sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new(args.Name);

            if (s_loadedAssemblies.TryGetValue(assemblyName, out Assembly? assembly))
            {
                return assembly;
            }

            foreach (Assembly loadedAssembly in s_assembliesWithResources)
            {
                string resourceName = $"SGF.Assembly::{assemblyName.Name}.dll";
                if (TryExtractingAssembly(loadedAssembly, resourceName, out Assembly? extractedAssembly))
                {
                    AddAssembly(extractedAssembly!);
                    return extractedAssembly!;
                };
            }

            return null;
        }


        /// <summary>
        /// Attempts to load an assembly that is contained within another assembly as a resource
        /// </summary>
        /// <param name="assembly">The assembly that should contain the resource</param>
        /// <param name="resourceName">The expected name of the resource</param>
        /// <param name="loadedAssembly">The assembly if it was loaded</param>
        /// <returns>True if the assembly could be loaded otherwise false</returns>
        private static bool TryExtractingAssembly(Assembly assembly, string resourceName, out Assembly? loadedAssembly)
        {
            loadedAssembly = null;
            if (TryGetResourceBytes(assembly, resourceName, out byte[]? assemblyBytes))
            {
                try 
                {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                    loadedAssembly = TryGetResourceBytes(assembly, Path.ChangeExtension(resourceName, ".pdb"), out byte[]? symbolBytes)
                        ? Assembly.Load(assemblyBytes!, symbolBytes!)
                        : Assembly.Load(assemblyBytes!);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
                    return true;
                }
                catch 
                {}
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
            ManifestResourceInfo? resourceInfo = assembly.GetManifestResourceInfo(resourceName);
            if (resourceInfo == null)
            {
                return false;
            }

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                {
                    return false;
                }

                bytes = new byte[stream.Length];
                _ = stream.Read(bytes, 0, bytes.Length);
            }

            return true;
        }
    }
}
""", Encoding.UTF8);
}



