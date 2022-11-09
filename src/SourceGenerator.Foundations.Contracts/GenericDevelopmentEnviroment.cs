using SGF.Contracts;
using SGF.Logging;
using SGF.Logging.Archives;
using System.Diagnostics;

namespace SGF
{
    /// <summary>
    /// A <see cref="IDevelopmentEnviroment"/> that does not do anything
    /// </summary>
    internal class GenericDevelopmentEnviroment : IDevelopmentEnviroment
    {
        private readonly ILogArchive m_archive;

        public GenericDevelopmentEnviroment(string assemblyName)
        {
            m_archive = new LogFileArchive(assemblyName);
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public bool AttachDebugger(int processId)
        {
            return Debugger.Launch();
        }

        /// <inheritdoc cref="IDevelopmentEnviroment"/>
        public ILogger GetLogger(string context)
        {
            return new Logger(context, m_archive);
        }
    }
}
