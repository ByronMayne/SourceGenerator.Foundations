#nullable enable
using Serilog;
using SGF.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static readonly bool s_loadedContractsAssembly;
        private static readonly ISet<Assembly> s_assemblies;
        private static readonly AssemblyName s_contractsAssemblyName;

        static AssemblyResolver()
        {
            s_assemblies = new HashSet<Assembly>
            {
                typeof(AssemblyResolver).Assembly
            };
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            s_contractsAssemblyName = new AssemblyName("SourceGenerator.Foundations.Contracts");
            s_loadedContractsAssembly = ResolveAssembly(s_contractsAssemblyName) != null;
        }

        [ModuleInitializer]
        internal static void InitializeResolver()
        {

            //OperatingSystem osVersion = Environment.OSVersion;

            //switch (osVersion.Platform)
            //{
            //    case PlatformID.Win32NT:
            //        break;

            //    case PlatformID.Unix:

            //        break;
            //}

            //// if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            // {
            //     //ResolveAssembly("SourceGenerator.Foundations.Windows");
            // }
            // //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // {
            //     // TODO: Linux support
            //     // ResolveAssembly("SourceGenerator.Foundations.Linux");
            // }
            // //else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            // {
            //     // TODO: OSX support
            //     // ResolveAssembly("SourceGenerator.Foundations.OSX");
            // }
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

                            return resolvedAssembly;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (assemblyName != s_contractsAssemblyName)
                        {
                            // This is redirected to a metho so that it does not attempt to
                            // load the assembly if it has failed.
                            Log(exception, LogLevel.Error, "Failed to load assembly {Assembly} due to exception", assemblyName);
                        }
                        return null;
                    }
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

            if (s_loadedContractsAssembly)
            {
                LogInternal(exception, LogLevel.Info, message, parameters);
            }
            else
            {
                string logPath = Path.Combine(Path.GetTempPath(), "SourceGenerator.Foundations");
                File.AppendAllLines(logPath,
                    new string[]
                    {
                        message,
                        exception?.ToString() ??"",
                    });
            }
        }
    }
}