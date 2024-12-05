using SGF.Diagnostics;
using System.Collections.Generic;

namespace SGF
{
    /// <summary>
    /// Contains information about the environment that this source generator is running in. It allows you to 
    /// star the debugger and get the custom platform loggers based on the context. 
    /// </summary>
    public interface IGeneratorEnvironment
    {
        /// <summary>
        /// Gets the name of the environment 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Attaches the debugger to the given process Id and returns back if it was successful or not. This can
        /// fail if Visual Studio is not already running
        /// </summary>
        bool AttachDebugger(int processId);

        /// <summary>
        /// Gets the list of sinks used for logging output
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ILogSink> GetLogSinks();
    }
}
