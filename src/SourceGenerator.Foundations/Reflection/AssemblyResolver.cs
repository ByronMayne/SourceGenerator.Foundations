#nullable enable
using SGF.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SGF.Reflection
{
    internal static class AssemblyResolver
    {
        private static readonly string s_logFilePath;
        private static readonly ISet<Assembly> s_assemblies;

        static AssemblyResolver()
        {
            Type resolverType = typeof(AssemblyResolver);
            Assembly hostAssembly = resolverType.Assembly;
            AssemblyName hostAssemblyName = hostAssembly.GetName();
            string logDirectory = Path.Combine(Path.GetTempPath(), "SourceGenerator.Foundations");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            s_logFilePath = Path.Combine(logDirectory, $"{hostAssemblyName.Name}.asm.log");


            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            s_assemblies = new HashSet<Assembly>
            {
                typeof(AssemblyResolver).Assembly
            };
        }

        [ModuleInitializer]
        internal static void InitializeResolver()
        {
            // .cctor is invoked before getting here
            try
            {
                ResolveAssembly(new AssemblyName("SourceGenerator.Foundations"));
                ResolveAssembly(new AssemblyName("SourceGenerator.Foundations.Contracts"));
                LoadDevelopmentEnvironment();
            }
            catch (Exception exception)
            {
                Debugger.Launch();
            }
        }

        /// <summary>
        ///  This has to be broken into it's own function can can't be under the Initialize resolver because
        ///  referencing the interface will fail.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadDevelopmentEnvironment()
        {
            string? platformAssemblyName = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformAssemblyName = "SourceGenerator.Foundations.Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformAssemblyName = "SourceGenerator.Foundations.Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformAssemblyName = "SourceGenerator.Foundations.Linux";
            }

            if (platformAssemblyName != null)
            {
                Log($"Loading platform assembly {platformAssemblyName}");
                Assembly? platformAssembly = ResolveAssembly(new AssemblyName(platformAssemblyName));

                if (platformAssembly != null)
                {
                    Type? devEnvType = platformAssembly.GetTypes()
                        .Where(typeof(IDevelopmentEnviroment).IsAssignableFrom)
                        .Where(t => !t.IsAbstract)
                        .FirstOrDefault();

                    if (devEnvType != null)
                    {
                        IDevelopmentEnviroment environment = (IDevelopmentEnviroment)Activator.CreateInstance(devEnvType);
                        DevelopmentEnviroment.SetEnvironment(environment);
                    }

                    Log($"Successfully loaded platform assembly {platformAssemblyName}");
                }
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
            string resourceName = $"{ResourceConfiguration.AssemblyResourcePrefix}{assemblyName.Name}.dll";

            foreach (Assembly loadedAssembly in s_assemblies)
            {
                if (string.Equals(loadedAssembly.GetName().Name, assemblyName.Name))
                {
                    return loadedAssembly;
                }
            }

            foreach (Assembly assembly in s_assemblies)
            {
                ManifestResourceInfo resourceInfo = assembly.GetManifestResourceInfo(resourceName);
                if (resourceInfo != null)
                {
                    using Stream stream = assembly.GetManifestResourceStream(resourceName);
                    byte[] data = new byte[stream.Length];
                    _ = stream.Read(data, 0, data.Length);
                    try
                    {
                        Assembly resolvedAssembly = Assembly.Load(data);

                        if (resolvedAssembly != null)
                        {
                            if (!s_assemblies.Contains(resolvedAssembly))
                            {
                                s_assemblies.Add(resolvedAssembly);
                            }
                            Log($"Loaded embedded assembly {assemblyName} from within {assembly.GetName().Name}");
                            return resolvedAssembly;
                        }
                    }
                    catch (Exception exception)
                    {
                        // This is redirected to a metho so that it does not attempt to
                        // load the assembly if it has failed.
                        Log($"Failed to load assembly {assemblyName} due to exception. \n{exception}");
                        return null;
                    }
                }
            }
            Log($"Failed to resolve assembly {assemblyName.FullName}");
            return null;
        }

        /// <summary>
        /// Wrapper around the logging implemention to handle the case where loading the contracts library can actually fail
        /// </summary>
        private static void Log(string message)
        {
            File.AppendAllLines(s_logFilePath,
                new string[]
                {
                     $"{DateTime.Now:hh:mm:ss} {message}",
                });
        }
    }
}