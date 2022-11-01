using SGF.Contracts;
using SGF.Diagnostics;
using SGF.Interop.VisualStudio;
using System;

namespace SGF
{
    /// <summary>
    /// Represents a enviroment where the user is authoring code in VSCode
    /// </summary>
    internal class VSCodeEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public void AttachDebugger(int processId)
        {
          
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public ILogger GetLogger(string context)
        {
            throw new NotImplementedException();
        }
    }
}
