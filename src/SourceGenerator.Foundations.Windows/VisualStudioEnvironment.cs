using SGF.Contracts;
using SGF.Interop.VisualStudio;
using SGF.Logging;

namespace SGF
{
    /// <summary>
    /// Represents a enviroment where the user is authoring code in Visual Studio 
    /// </summary>
    internal class VisualStudioEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            VisualStudioInterop.AttachDebugger();
            return true;
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public ILogger GetLogger(string context)
        {
            VisualStudioLogger logger = new VisualStudioLogger(context);
            return logger;
        }
    }
}
