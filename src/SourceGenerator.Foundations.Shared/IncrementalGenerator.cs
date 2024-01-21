using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using SGF.Diagnostics;
using SGF.Reflection;
using SGF.Diagnostics.Sinks;
using Microsoft.CodeAnalysis;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince.
    /// </summary>
    internal abstract class IncrementalGenerator : IIncrementalGenerator, IDisposable
    {
        private static readonly IDevelopmentPlatform? s_developmentPlatform;
        protected static readonly AppDomain s_currentDomain;

        /// <summary>
        /// Gets the name of the source generator
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the log that can allow you to output information to your
        /// IDE of choice
        /// </summary>
        public ILogger Logger { get; }


        static IncrementalGenerator()
        {
            AssemblyResolver.Initialize();
            s_currentDomain = AppDomain.CurrentDomain;
            s_developmentPlatform = GetPlatform();

            s_currentDomain.UnhandledException += (c, d) => Debugger.Launch();
        }


        /// <summary>
        /// Initializes a new instance of the incremental generator with an optional name
        /// </summary>
        protected IncrementalGenerator(string? name)
        {
            Name = name ?? GetType().Name;
            Logger = CreateLogger(Name);
            //Logger.Debug("Initalizing {GeneratorName}", name ?? GetType().Name);
            s_currentDomain.ProcessExit += OnProcessExit;
        }

        private ILogger CreateLogger(string sourceContext)
        {
            Logger logger = new(sourceContext);

            if (s_developmentPlatform != null)
            {
                foreach (ILogSink sink in s_developmentPlatform.GetLogSinks())
                {
                    logger.AddSink(sink);
                }
            }

            if (Environment.UserInteractive)
            {
                logger.AddSink<ConsoleSink>();
            }

            return logger;
        }

        /// <summary>
        /// Implement to initalize the incremental source generator
        /// </ summary >
        protected abstract void OnInitialize(SgfInitializationContext context);

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
            _ = (s_developmentPlatform?.AttachDebugger(process.Id));
        }

        /// <summary>
        /// Raised when one of the generator functions throws an unhandle exception. Override this to define your own behaviour 
        /// to handle the exception. 
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
        protected virtual void OnException(Exception exception)
        {
            Logger.Error(exception, $"Unhandled exception was throw while running the generator {Name}");
        }

        /// <summary>
        /// Raised when the process is closing, giving us a chance to cleanup any resources
        /// </summary>
        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                Dipose();
                s_currentDomain.ProcessExit -= OnProcessExit;
                s_currentDomain.UnhandledException -= OnUnhandledException;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Exception thrown while dispoing '{Name}'");
            }
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
        private static IDevelopmentPlatform? GetPlatform()
        {
            string? platformAssembly = null;
            IDevelopmentPlatform? platform = null;
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
                        .Where(typeof(IDevelopmentPlatform).IsAssignableFrom)
                        .FirstOrDefault();
                    if (platformType != null)
                    {
                        platform = Activator.CreateInstance(platformType) as IDevelopmentPlatform;
                    }
                }
                catch
                { }
            }

            return platform;
        }

        /// <inheritdoc cref="IDisposable"/>
        void IDisposable.Dispose()
        {
            Dipose();
        }

        /// <inheritdoc cref = "IIncrementalGenerator" />
        void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                SgfInitializationContext sgfContext = new(context, Logger, OnException);

                OnInitialize(sgfContext);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"Error! An unhandle exception was thrown while initializing the source generator '{Name}'.");
            }
        }
    }
}
