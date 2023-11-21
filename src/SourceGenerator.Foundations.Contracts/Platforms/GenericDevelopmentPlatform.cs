using SGF.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SGF.Platforms
{
    internal class GenericDevelopmentPlatform : IDevelopmentPlatform
    {
        public bool AttachDebugger(int processId)
        {
            return Debugger.Launch();
        }

        public IEnumerable<ILogSink> GetLogSinks()
        {
            return Array.Empty<ILogSink>();
        }
    }
}
