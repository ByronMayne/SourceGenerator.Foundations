
using Serilog.Core;
using SGF.Interop.VisualStudio;
using System.Collections.Generic;
using Debugger = System.Diagnostics.Debugger;

namespace SGF
{
    internal class WindowsDevelopmentEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(bool @break)
        {
            if (VisualStudioInterop.HasEnvironment)
            {
                VisualStudioInterop.AttachDebugger(@break);
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
