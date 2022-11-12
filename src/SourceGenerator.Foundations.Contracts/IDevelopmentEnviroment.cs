using Serilog.Core;
using System.Collections.Generic;

namespace SGF
{
    /// <summary>
    /// Abstracts for development environments supported by the platform
    /// </summary>
    public interface IDevelopmentEnviroment
    {
        /// <summary>
        /// Attaches the debugger to the given process Id
        /// </summary>
        bool AttachDebugger(int processId);

        /// <summary>
        /// Gets the list of sinks used for logging output
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ILogEventSink> GetLogSinks();
    }
}
