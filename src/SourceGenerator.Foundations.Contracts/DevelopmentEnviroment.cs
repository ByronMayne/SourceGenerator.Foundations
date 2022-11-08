using SGF.Contracts;
using SGF.Diagnostics;
using SGF.NoOp;
using System;
using System.Collections.Generic;

namespace SGF
{
    /// <summary>
    /// Static wrapper around the current development environment
    /// </summary>
    public static class DevelopmentEnviroment
    {
        private static readonly Dictionary<string, ILogger> s_loggers;

        /// <summary>
        /// Gets the currently active development enviroment
        /// </summary>
        public static IDevelopmentEnviroment Instance { get; }

        static DevelopmentEnviroment()
        {
            s_loggers = new Dictionary<string, ILogger>();
            Instance = new GenericDevelopmentEnviroment();
            AppDomain.CurrentDomain.UnhandledException += OnExceptionThrown;
        }

        /// <summary>
        /// Attaches the debugger to the given process Id
        /// </summary>
        public static void AttachDebugger(int processId)
            => Instance.AttachDebugger(processId);

        /// <summary>
        /// Gets or creates a new logger used to output information from the source generator 
        /// </summary>
        /// <param name="context">A string context that will be used to label the logger</param>
        public static ILogger GetLogger(string context)
        {
            if (s_loggers.TryGetValue(context, out ILogger? logger))
            {
                return logger;
            }
            logger = Instance.GetLogger(context);
            s_loggers[context] = logger;
            return logger;
        }

        /// <summary>
        /// Invoked whenver an exception happens in the source generator. Normally this would just
        /// crash the generator and it would get lost.
        /// </summary>
        private static void OnExceptionThrown(object sender, UnhandledExceptionEventArgs e)
        {
            foreach(ILogger logger in s_loggers.Values)
            {
                logger.LogError((Exception)e.ExceptionObject, "Unhandle exception was thrown in the app domain");
            }
        }
    }
}
