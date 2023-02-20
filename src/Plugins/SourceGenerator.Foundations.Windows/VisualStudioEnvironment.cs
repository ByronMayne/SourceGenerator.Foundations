using Serilog.Core;
using SGF.Interop.VisualStudio;
using System.Collections.Generic;
using System.Diagnostics;

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
        public IEnumerable<ILogEventSink> GetLogSinks()
        {
            yield return new VisualStudioLogEventSink();
        }
    }
}
