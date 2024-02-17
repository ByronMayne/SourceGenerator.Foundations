using System;
using System.Diagnostics;
using SGF.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SGF
{
    /// <summary>
    /// Used as a base class for creating your own source generator. This class provides some helper
    /// methods and impoved debugging expereince. The generator that implements this must apply the 
    /// <see cref="GeneratorAttribute"/> but not inheirt from <see cref="IIncrementalGenerator"/>
    /// </summary>
    public abstract class IncrementalGenerator : IDisposable
    {
        private readonly IGeneratorEnvironment m_developmentPlatform;

        /// <summary>
        /// Gets the name of the source generator
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
        protected IncrementalGenerator(
            string? name,
            IGeneratorEnvironment developmentPlatform,
            ILogger logger)
        {
            m_developmentPlatform = developmentPlatform;
            Name = name ?? GetType().Name;
            Logger = logger;
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
            m_developmentPlatform.AttachDebugger(process.Id);
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
        /// Events raised when the exception is being thrown by the app domain 
        /// </summary>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                OnException(exception);
            }
        }

        /// <inheritdoc cref="IDisposable"/>
        void IDisposable.Dispose()
        {
            Dipose();
        }
    }
}