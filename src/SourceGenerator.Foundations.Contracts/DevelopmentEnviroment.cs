using Serilog;
using Serilog.Core;
using SGF.Sinks;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SGF
{
    /// <summary>
    /// Static wrapper around the current development environment
    /// </summary>
    public static class DevelopmentEnviroment
    {
        private static readonly LogEventSinkAggregate m_sinkAggregate;

        /// <summary>
        /// Gets the currently active development enviroment
        /// </summary>
        public static IDevelopmentEnviroment Instance { get; private set; }

        /// <summary>
        /// Gets the temp directory where generators can store data
        /// </summary>
        public static string TempDirectory { get; }

        static DevelopmentEnviroment()
        {
            m_sinkAggregate = new LogEventSinkAggregate();

            TempDirectory = Path.Combine(Path.GetTempPath(), "SourceGenerator.Foundations");
            Instance = new GenericDevelopmentEnviroment();
            AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;
        }

        [ModuleInitializer]
        internal static void Initialize()
        {
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            string logPath = Path.Combine(TempDirectory, "SourceGenerator.Foundations.log");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, shared: true, retainedFileCountLimit: 1, buffered: false)
                .WriteTo.Sink(m_sinkAggregate)
                .CreateLogger();
        }

        /// <summary>
        /// Attaches the debugger to the given process Id
        /// </summary>
        [DebuggerStepThrough]
        public static bool AttachDebugger(bool @break)
        {
            return Instance.AttachDebugger(@break);
        }

        /// <summary>
        /// Invoked whenver an exception happens in the source generator. Normally this would just
        /// crash the generator and it would get lost.
        /// </summary>
        private static void OnExceptionThrown(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject as Exception, "An unhandled exception was thrown by sender {Sender}", sender);
        }

        /// <summary>
        /// Sets the active development environment  
        /// </summary>
        /// <typeparam name="T">The type of development environment</typeparam>
        public static void SetEnvironment(IDevelopmentEnviroment developmentEnviroment)
        {
            Log.Information("Changing Development Enviroment to {Type}", developmentEnviroment.GetType().FullName);
            Instance = developmentEnviroment;
            m_sinkAggregate.AddRange(developmentEnviroment.GetLogSinks());
        }
    }
}
