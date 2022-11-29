using EnvDTE;
using Serilog.Core;
using SGF.Interop.VisualStudio;
using System.Collections.Generic;

namespace SGF
{
    internal class WindowsDevelopmentEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            if (VisualStudioInterop.HasEnvironment)
            {
                VisualStudioInterop.AttachDebugger();
                return true;
            }
            // TODO: VSCode
            // TODO: Cli
            return false;
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public IEnumerable<ILogEventSink> GetLogSinks()
        {
            if (VisualStudioInterop.HasEnvironment)
            {
                yield return new VisualStudioLogEventSink();
            }
            // TODO: VSCode
            // TODO: Cli
        }
    }
}
