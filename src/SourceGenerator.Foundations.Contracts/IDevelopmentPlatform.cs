using SGF.Diagnostics;
using System.Collections.Generic;

namespace SGF
{
    /// <summary>
    /// Abstracts for development environments supported by the platform
    /// </summary>
    public interface IDevelopmentPlatform
    {
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
