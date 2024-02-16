using SGF.Diagnostics;
using SGF.Interop.VisualStudio;
using System;
using System.Collections.Generic;

namespace SGF
{

    /// <summary>
    /// Represents a enviroment where the user is authoring code in Visual Studio 
    /// </summary>
    internal class WindowsDevelopmentPlatform : IGeneratorEnvironment
    {
        public PlatformType Type { get; }

        public string Name { get; }


        public WindowsDevelopmentPlatform()
        {
            Name = "VisualStudio";
            Type = PlatformType.VisualStudio;

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VisualStudioVersion")))
            {
                Type = PlatformType.VisualStudio;
            }
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            return Type switch
            {
                PlatformType.VisualStudio => VisualStudioInterop.AttachDebugger(),
                _ => true,
            };
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public IEnumerable<ILogSink> GetLogSinks()
        {
            switch (Type)
            {
                case PlatformType.VisualStudio:
                    yield return new VisualStudioLogEventSink();
                    break;
            }
        }
    }
}
