#nullable enable
using Microsoft.CodeAnalysis;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince.
    /// </summary>
    internal abstract class IncrementalGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Gets the name of the generator used for logging
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the log that can allow you to output information to your
        /// IDE of choice
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the incremental generator with an optional name
        /// </summary>
        protected IncrementalGenerator(string? name = "")
        {
            Type type = GetType();
            Logger = Log.Logger.ForContext(type);

            if (string.IsNullOrEmpty(name))
            {
                name = type.Name;
            }
            Name = name!;
            Logger.Information("Initalizing Generator {Name}", Name);
        }

        /// <summary>
        /// Attaches the debugger automtically if you are running from Visual Studio. You have the option
        /// to stop or just continue
        /// </summary>
        [DebuggerStepThrough]
        protected void AttachDebugger(bool breakHere = true)
        {
            _ = DevelopmentEnviroment.AttachDebugger(breakHere);
   
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
