#nullable enable
using Microsoft.CodeAnalysis;
using Serilog;
using Serilog.Core;
using SGF.Reflection;
using System;
using System.Diagnostics;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince.
    /// </summary>
    internal abstract class IncrementalGenerator : IIncrementalGenerator
    {
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
        }

        /// <summary>
        /// Initializes a new instance of the incremental generator with an optional name
        /// </summary>
        protected IncrementalGenerator(string? name)
        {
            Name = name ?? GetType().Name;
            Logger = DevelopmentEnviroment.Logger.ForContext(Constants.SourceContextPropertyName, Name);
            Logger.Debug("Initalizing {GeneratorName}", name ?? GetType().Name);
        }

        /// <summary>
        /// Attaches the debugger automtically if you are running from Visual Studio. You have the option
        /// to stop or just continue
        /// </summary>
        protected void AttachDebugger()
        {
            Process process = Process.GetCurrentProcess();
            _ = DevelopmentEnviroment.AttachDebugger(process.Id);
        }

        /// <inheritdoc cref = "IIncrementalGenerator" />
        void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                SgfInitializationContext sgfContext = new SgfInitializationContext(context, OnException);

                OnInitialize(sgfContext);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error! An unhandle exception was thrown while initializing the source generator '{Name}'.", Name);
            }
        }

        /// <summary>
        /// Raised when one of the generator functions throws an unhandle exception. Override this to define your own behaviour 
        /// to handle the exception. 
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
        protected virtual void OnException(Exception exception)
        {
            Logger.Error(exception, "Unhandled exception was throw while running the generator {Name}", Name);
        }

        /// <summary>
        /// Implement to initalize the incremental source generator
        /// </ summary >
        protected abstract void OnInitialize(SgfInitializationContext context);
    }
}
