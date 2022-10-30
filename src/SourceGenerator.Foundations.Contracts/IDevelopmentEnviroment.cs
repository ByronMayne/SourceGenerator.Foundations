using SGF.Diagnostics;

namespace SGF.Contracts
{
    /// <summary>
    /// Abstracts for development environments supported by the platform
    /// </summary>
    public interface IDevelopmentEnviroment
    {
        /// <summary>
        /// Attaches the debugger to the given process Id
        /// </summary>
        void AttachDebugger(int processId);

        /// <summary>
        /// Gets or creates a new logger used to output information from the source generator 
        /// </summary>
        /// <param name="context">A string context that will be used to label the logger</param>
        ILogger GetLogger(string context);
    }
}
