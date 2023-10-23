#nullable enable
using SGF.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SGF.Reflection
{
    internal static class AssemblyResolver
    {
        private enum LogLevel
        {
            Info,
            Error,
            Warning
        }

        private static readonly IList<Assembly> s_assemblies;
        private static readonly AssemblyName s_contractsAssemblyName;
        private static readonly string s_unpackDirectory;

        static AssemblyResolver()
        {
            s_assemblies = new List<Assembly>();
            s_unpackDirectory = Path.Combine(Path.GetTempPath(), "SourceGenerator.Foundations", "Assemblies");
            s_contractsAssemblyName = new AssemblyName();
            if (!Directory.Exists(s_unpackDirectory))
            {
                Directory.CreateDirectory(s_unpackDirectory);
            }
        }

        [ModuleInitializer]
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
                currentDomain.AssemblyResolve += OnResolveAssembly;
                currentDomain.AssemblyLoad += OnAssemblyLoaded;

                foreach (Assembly assembly in currentDomain.GetAssemblies())
                {
                    if (!s_assemblies.Contains(assembly))
                    {
                        s_assemblies.Add(assembly);
                    }
                }
            }
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            if (!s_assemblies.Contains(args.LoadedAssembly))
            {
                s_assemblies.Add(args.LoadedAssembly);
            }
        }

        /// <summary>
        /// Attempts to resolve any assembly by looking for dependencies that are embedded directly
        /// in this dll.
        /// </summary>
        private static Assembly? OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new(args.Name);
            return ResolveAssembly(assemblyName);
        }

        private static Assembly? ResolveAssembly(AssemblyName assemblyName)
        {
            for (int i = 0; i < s_assemblies.Count; i++)
            {
                Assembly assembly = s_assemblies[i];
                if (AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName()))
                {
                    return assembly;
                }
            }

            string resourceName = $"{ResourceConfiguration.AssemblyResourcePrefix}{assemblyName.Name}.dll";

            for (int i = 0; i < s_assemblies.Count; i++)
            {
                Assembly assembly = s_assemblies[i];

                if (assembly.IsDynamic)
                {
                    // Dynamic assemblies don't have reosurces and throw exceptions if you try to access them.
                    continue;
                }

                ManifestResourceInfo resourceInfo = assembly.GetManifestResourceInfo(resourceName);
                if (resourceInfo != null)
                {
                    string assemblyPath = Path.Combine(s_unpackDirectory, $"{assemblyName.Name}-{assemblyName.Version}.dll");

                    if (!File.Exists(assemblyPath))
                    {
                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                        using (FileStream fileStream = new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                        {
                            resourceStream.CopyTo(fileStream);
                            fileStream.Flush();
                        }
                    }
                    Assembly resolvedAssembly = Assembly.LoadFile(assemblyPath);
                    s_assemblies.Add(resolvedAssembly);
                    return resolvedAssembly;
                }
            }
            return null;
        }

        /// <summary>
        /// Wrapper around the logging implemention to handle the case where loading the contracts library can actually fail
        /// </summary>
        private static void Log(Exception? exception, LogLevel level, string message, params object?[]? parameters)
        {
            /// <summary>
            /// This indirection might seem a bit weird but it's because we want to log output from the assembly resolver
            /// however since the logging library is defined within `SourceGenerator.Foundations.Contracts` if that assembly
            /// fails to load we will create a stake overflow since calling to the logger will try to load the assembly again. 
            /// We issoloate the logging in this function so the runtime does not attempt to load it directrly 
            /// </summary>
            static void LogInternal(Exception? exception, LogLevel level, string message, object?[]? parameters)
            {
                switch (level)
                {
                    case LogLevel.Info:
                        DevelopmentEnviroment.Logger.Information(exception, message, parameters);
                        break;
                    case LogLevel.Warning:
                        DevelopmentEnviroment.Logger.Warning(exception, message, parameters);
                        break;
                    case LogLevel.Error:
                        DevelopmentEnviroment.Logger.Error(exception, message, parameters);
                        break;
                }
            }

            LogInternal(exception, LogLevel.Info, message, parameters);
        }
    }
}