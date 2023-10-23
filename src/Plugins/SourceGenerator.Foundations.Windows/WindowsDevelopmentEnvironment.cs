using Serilog.Core;
using SGF.Interop.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SGF
{

    /// <summary>
    /// Represents a enviroment where the user is authoring code in Visual Studio 
    /// </summary>
    internal class WindowsDevelopmentEnvironment : IDevelopmentEnviroment
    {
        public EnvironmentType Type { get; }


        public WindowsDevelopmentEnvironment()
        {
            Type = EnvironmentType.VisualStudio;

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VisualStudioVersion")))
            {
                Type = EnvironmentType.VisualStudio;
            }
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            switch (Type)
            {
                case EnvironmentType.VisualStudio:
                    VisualStudioInterop.AttachDebugger();
                    break;
            }
            return true;
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public IEnumerable<ILogEventSink> GetLogSinks()
        {
            switch (Type)
            {
                case EnvironmentType.VisualStudio:
                    yield return new VisualStudioLogEventSink();
                    break;
            }
        }
    }
}
