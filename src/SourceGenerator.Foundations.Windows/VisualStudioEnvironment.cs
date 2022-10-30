using SGF.Contracts;
using System;

namespace SGF
{
    /// <summary>
    /// Represents a enviroment where the user is authoring code in Visual Studio 
    /// </summary>
    internal class VisualStudioEnvironment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public void AttachDebugger(int processId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public void WriteOutput(string message)
        {
            throw new NotImplementedException();
        }
    }
}
