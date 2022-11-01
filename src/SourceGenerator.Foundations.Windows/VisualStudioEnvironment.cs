using SGF.Contracts;
using SGF.Diagnostics;
using SGF.Interop.VisualStudio;

namespace SGF
{
    /// <summary>
    /// Represents a enviroment where the user is authoring code in Visual Studio 
    /// </summary>
    internal class VisualStudioEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public void AttachDebugger(int processId)
        {
            VisualStudioInterop.AttachDebugger();
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public ILogger GetLogger(string context)
        {
            VisualStudioLogger logger = new VisualStudioLogger(context);
            return logger;
        }
    }
}
