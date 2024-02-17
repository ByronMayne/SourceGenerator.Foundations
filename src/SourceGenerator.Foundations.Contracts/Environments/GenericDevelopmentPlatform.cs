using SGF.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SGF.Environments
{
    public class GenericDevelopmentEnvironment : IGeneratorEnvironment
    {
        public string Name { get; }

        public GenericDevelopmentEnvironment()
        {
            Name = "Generic";
        }

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
