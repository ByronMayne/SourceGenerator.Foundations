using Serilog.Core;
using System;
using System.Collections.Generic;

namespace SGF
{
    /// <summary>
    /// Represents a enviroment where the user is authoring code in VSCode
    /// </summary>
    internal class VSCodeEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            return false;
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public IEnumerable<ILogEventSink> GetLogSinks()
        {
            return Array.Empty<ILogEventSink>();
        }
    }
}
