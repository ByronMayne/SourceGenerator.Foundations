using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Shims
{
    internal class BuildEngineShim : IBuildEngine
    {
        public bool ContinueOnError => false;
        public int LineNumberOfTaskNode => 0;
        public int ColumnNumberOfTaskNode => 0;
        public string ProjectFileOfTaskNode => string.Empty;
        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
        {
            return true;
        }

        public void LogCustomEvent(CustomBuildEventArgs e) { }
        public void LogErrorEvent(BuildErrorEventArgs e) { }
        public void LogMessageEvent(BuildMessageEventArgs e) { }
        public void LogWarningEvent(BuildWarningEventArgs e) { }
    }
}
