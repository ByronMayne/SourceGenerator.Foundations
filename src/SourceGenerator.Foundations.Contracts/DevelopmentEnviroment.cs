using Serilog;
using SGF.Sinks;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SGF
{
    /// <summary>
    /// Static wrapper around the current development environment
    /// </summary>
    public static class DevelopmentEnviroment
    {
        private static readonly LogEventSinkAggregate s_sinkAggregate;

        /// <summary>
        /// Gets the currently active development enviroment
        /// </summary>
        public static IDevelopmentEnviroment Instance { get; }

        /// <summary>
        /// Gets the temp directory where generators can store data
        /// </summary>
        public static string TempDirectory { get; }

        /// <summary>
        /// Gets the logger that was created
        /// </summary>
        public static ILogger Logger { get; }

        static DevelopmentEnviroment()
        {
            s_sinkAggregate = new LogEventSinkAggregate();

            TempDirectory = Path.Combine(Path.GetTempPath(), "SourceGenerator.Foundations");

            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            string logPath = Path.Combine(TempDirectory, "SourceGenerator.Foundations.log");

            Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, retainedFileCountLimit: 1, buffered: false, shared: true)
                .WriteTo.Sink(s_sinkAggregate)
                .CreateLogger();

            Instance = new GenericDevelopmentEnviroment();
            AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;

            Type? developmentEnvironment = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assembly windowsEnvironmentAssembly = Assembly.Load(new AssemblyName("SourceGenerator.Foundations.Windows"));

                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VisualStudioVersion")))
                {
                    developmentEnvironment = windowsEnvironmentAssembly.GetType("SGF.VisualStudioEnvironment");
                }
            }
      
            if (developmentEnvironment != null)
            {
                Instance = (IDevelopmentEnviroment)Activator.CreateInstance(developmentEnvironment, true);
                s_sinkAggregate.Add(Instance.GetLogSinks());
            }
        }


        /// <summary>
        /// Attaches the debugger to the given process Id
        /// </summary>
        public static bool AttachDebugger(int processId)
        {
            return Instance.AttachDebugger(processId);
        }

        /// <summary>
        /// Invoked whenver an exception happens in the source generator. Normally this would just
        /// crash the generator and it would get lost.
        /// </summary>
        private static void OnExceptionThrown(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject as Exception, "An unhandled exception was thrown by sender {Sender}", sender);
        }
    }
}
