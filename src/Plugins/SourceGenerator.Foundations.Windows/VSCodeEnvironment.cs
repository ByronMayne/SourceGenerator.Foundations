using SGF.Contracts;
using SGF.Interop.VisualStudio;
using SGF.Logging;
using System;

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
        public ILogger GetLogger(string context)
        {
            throw new NotImplementedException();
        }
    }
}
