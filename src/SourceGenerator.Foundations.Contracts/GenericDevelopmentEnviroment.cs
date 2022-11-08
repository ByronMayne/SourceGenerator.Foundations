using SGF.Contracts;
using SGF.Diagnostics;
using System.Diagnostics;

namespace SGF.NoOp
{
    /// <summary>
    /// A <see cref="IDevelopmentEnviroment"/> that does not do anything
    /// </summary>
    internal class GenericDevelopmentEnviroment : IDevelopmentEnviroment
    {
        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public void AttachDebugger(int processId)
        {
            Debugger.Launch();
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public ILogger GetLogger(string context)
            => new FileLogger(context);
    }
}
