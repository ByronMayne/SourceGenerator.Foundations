#nullable enable
using Microsoft.CodeAnalysis;
using Serilog;
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
        /// Gets the log that can allow you to output information to your
        /// IDE of choice
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the incremental generator with an optional name
        /// </summary>
        protected IncrementalGenerator(string? name = null)
        {
            Logger = DevelopmentEnviroment.Logger.ForContext(GetType());
            Logger.Information("Initalizing {GeneratorName}", name ?? GetType().Name);
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
                OnInitialize(context);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error! An unhandle exception was thrown while running the source generator.");
            }
        }

        /// <summary>
        /// Implement to initalize the incremental source generator
        /// </ summary >
        protected abstract void OnInitialize(IncrementalGeneratorInitializationContext context);
    }
}
