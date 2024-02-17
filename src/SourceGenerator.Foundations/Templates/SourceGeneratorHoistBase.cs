
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace SGF.Templates;

public static class SourceGeneratorHoistBase
{
    public static SourceText RenderTemplate(string @namespace) => SourceText.From($$"""
#nullable enable
using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SGF.Environments;
using SGF.Diagnostics;
using SGF.Diagnostics.Sinks;

namespace {{@namespace}}
{
    /// <summary>
    /// Provides logic for hooking into the app domain to resolve assemblies as well
    // as capture exceptions and handle shutdown events 
    /// </summary>
    internal abstract class SourceGeneratorHoist
    {
        protected static readonly AppDomain s_currentDomain;

        protected readonly ILogger m_logger;
        // Needs to be an 'object' otherwise the assemblies will be attempted go be loaded before our assembly resolver
        protected readonly object m_environment; 

        /// <summary>
        /// Gets the name of the source generator for logging purposes 
        /// </summary>
        public string Name { get; }

        static SourceGeneratorHoist()
        {
            AssemblyResolver.Initialize();
            s_currentDomain = AppDomain.CurrentDomain;
        }

        protected SourceGeneratorHoist(string name)
        {
            Name = name;
            IGeneratorEnvironment environment  = GetEnvironment();
            m_environment = environment;
            s_currentDomain.ProcessExit += OnProcessExit;

            m_logger = new Logger(Name);
            if(environment != null)
            {
                foreach(ILogSink logSink in environment.GetLogSinks())
                {
                    m_logger.AddSink(logSink);
                }
            }

            if (Environment.UserInteractive)
            {
                m_logger.AddSink<ConsoleSink>();
            }
        }

        protected virtual void Dispose()
        {
            s_currentDomain.ProcessExit -= OnProcessExit;
        }

        /// <summary>
        /// Raised when the process is closing, giving us a chance to cleanup any resources
        /// </summary>
        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                m_logger.Error(ex, $"Exception thrown while disposing '{Name}'");
            }
        }

        /// <summary>
        /// Gets an instance of a development platform to be used to log and debug info
        /// </summary>
        /// <returns></returns>
        private static IGeneratorEnvironment GetEnvironment()
        {
            string? platformAssembly = null;
            IGeneratorEnvironment? platform = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows Development Platform
                platformAssembly = "SourceGenerator.Foundations.Windows";
            }
            else
            {
                // Generic Development Platform
                platformAssembly = "SourceGenerator.Foundations.Contracts";
            }

            if (!string.IsNullOrWhiteSpace(platformAssembly))
            {
                AssemblyName assemblyName = new(platformAssembly);
                Assembly? assembly = null;
                try
                {
                    assembly = Assembly.Load(platformAssembly);
                    Type? platformType = assembly?
                        .GetTypes()
                        .Where(typeof(IGeneratorEnvironment).IsAssignableFrom)
                        .FirstOrDefault();
                    if (platformType != null)
                    {
                        platform = Activator.CreateInstance(platformType) as IGeneratorEnvironment;
                    }
                }
                catch
                { }
            }

            return platform ?? new GenericDevelopmentEnvironment();
        }
    }
}
""", Encoding.UTF8);
}



