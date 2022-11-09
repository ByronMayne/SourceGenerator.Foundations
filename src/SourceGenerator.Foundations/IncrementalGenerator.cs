#nullable enable

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using SGF.Logging;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince.
    /// </summary>
    public abstract class IncrementalGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Gets the log that can allow you to output information to your
        /// IDE of choice
        /// </summary>
        public ILogger Log { get; }

        /// <summary>
        /// Gets the name of the source generator which is used for logging
        /// </summary>
        public string GeneratorName { get; }

        /// <summary>
        /// Initializes a new instance of the incremental generator with an optional name
        /// </summary>
        protected IncrementalGenerator(string? name = null)
        {
            GeneratorName = name == null ? GetType().Name : name;
            Log = DevelopmentEnviroment.GetLogger(GeneratorName);
        }

        /// <summary>
        /// Attaches the debugger automtically if you are running from Visual Studio. You have the option
        /// to stop or just continue
        /// </summary>
        protected void AttachDebugger()
        {
            Process process = Process.GetCurrentProcess();
            DevelopmentEnviroment.AttachDebugger(process.Id);
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
                Log.LogError(exception, "Error! An unhandle exception was thrown while running the source generator.");
            }
        }

        /// <summary>
        /// Implement to initalize the incremental source generator
        /// </ summary >
        protected abstract void OnInitialize(IncrementalGeneratorInitializationContext context);
    }
}
