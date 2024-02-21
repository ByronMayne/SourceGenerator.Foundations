﻿using System;
using System.Diagnostics;
using SGF.Diagnostics;
using Microsoft.CodeAnalysis;
using SGF.Environments;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using SGF.Diagnostics.Sinks;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince. The generator that implements this must apply the 
    /// <see cref="GeneratorAttribute"/> but not inheirt from <see cref="IIncrementalGenerator"/>
    /// </summary>
    public abstract class IncrementalGenerator : IDisposable
    {
        private readonly static IGeneratorEnvironment s_environment;

        /// <summary>
        /// Gets the name of the source generator
        /// </summary>
        public string Name { get; private set; } 

        /// <summary>
        /// Gets the log that can allow you to output information to your
        /// IDE of choice
        /// </summary>
        public ILogger Logger { get; private set; } // Set with reflection, don't change 


        static IncrementalGenerator()
        {
            s_environment = CreateEnvironment();
        }

        /// <summary>
        /// Initializes a new instance of the incremental generator. Note both <paramref name="developmentPlatform"/>
        /// and <paramref name="logger"/> will be provided by the framework.
        /// </summary>
        protected IncrementalGenerator(string? name)
        {
            Name = name ?? GetType().Name;
            Logger = new Logger(Name);
            if(s_environment != null)
            {
                foreach(ILogSink sink in s_environment.GetLogSinks())
                {
                    Logger.AddSink(sink);
                }
            }
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            if (Environment.UserInteractive)
            {
                Logger.AddSink<ConsoleSink>();
            }
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        }

        /// <summary>
        /// Implement to initalize the incremental source generator
        /// </ summary >
        public abstract void OnInitialize(SgfInitializationContext context);

        /// <summary>
        /// Override to add logic for disposing this instance and free resources
        /// </summary>
        protected virtual void Dipose()
        { }

        /// <summary>
        /// Attaches the debugger automtically if you are running from Visual Studio. You have the option
        /// to stop or just continue
        /// </summary>
        protected void AttachDebugger()
        {
            Process process = Process.GetCurrentProcess();
            s_environment.AttachDebugger(process.Id);
        }

        /// <summary>
        /// Raised when one of the generator functions throws an unhandle exception. Override this to define your own behaviour 
        /// to handle the exception. 
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
        public virtual void OnException(Exception exception)
        {
            Logger.Error(exception, $"Unhandled exception was throw while running the generator {Name}");
        }

        /// <summary>
        /// Events raised when the exception is being thrown by the app domain 
        /// </summary>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                OnException(exception);
            }
        }

        /// <summary>
        /// Gets an instance of a development platform to be used to log and debug info
        /// </summary>
        /// <returns></returns>
        private static IGeneratorEnvironment CreateEnvironment()
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

        /// <inheritdoc cref="IDisposable"/>
        void IDisposable.Dispose()
        {
            Dipose();
        }
    }
}