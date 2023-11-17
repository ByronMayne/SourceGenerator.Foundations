using Serilog;
using SGF.Sinks;
using System;
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
        /// Gets the logger that was created
        /// </summary>
        public static ILogger Logger { get; }

        static DevelopmentEnviroment()
        {
            s_sinkAggregate = new LogEventSinkAggregate();

            LoggerConfiguration configuration = new LoggerConfiguration()
                .WriteTo.Sink(s_sinkAggregate);

            if (Environment.UserInteractive)
            {
                configuration.WriteTo.Console();
            }
            Logger = configuration.CreateLogger();
            string assemblyVersion
                = typeof(DevelopmentEnviroment)
                .Assembly
                .GetName()
                .Version.ToString();

            Instance = new GenericDevelopmentEnviroment();
            AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;

            try
            {

                Assembly? environmentAssembly = null;
                Type? developmentEnvironment = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AssemblyName windowsAssemblyName = new AssemblyName("SourceGenerator.Foundations.Windows");
                    environmentAssembly = Assembly.Load(windowsAssemblyName);
                }

                if(environmentAssembly != null)
                {
                    developmentEnvironment = environmentAssembly
                        .GetTypes()
                        .Where(typeof(IDevelopmentEnviroment).IsAssignableFrom)
                        .FirstOrDefault();
                }

                if (developmentEnvironment != null)
                {
                    Instance = (IDevelopmentEnviroment)Activator.CreateInstance(developmentEnvironment, true);
                    s_sinkAggregate.Add(Instance.GetLogSinks());
                }
            }
            catch
            {
                // Do nothing
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
