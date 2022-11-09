using SGF.Logging.Archives;
using System;

namespace SGF.Logging
{
    internal class Logger : ILogger
    {
        private readonly ILogArchive m_archive;
        private readonly string m_context;

        public Logger(string context, ILogArchive archive)
        {
            m_archive = archive;
            m_context = context;
        }

        public void Log(LogLevel logLevel, Exception? exception, string messageTemplate, object[] arguments)
        {
            string renderedMessage = messageTemplate;

            try
            {
                renderedMessage = string.Format(messageTemplate, arguments);

            }
            catch (Exception formatException)
            {
                m_archive.Write($"Exception thrown while rendering {formatException}");
            }
            string exceptionMessage = exception == null
                ? ""
                : $"\n{exception}";

            m_archive.Write($"{DateTime.Now:hh:mm:ss} [{logLevel.GetMoniker()}] {m_context} {renderedMessage}{exceptionMessage}");
        }
    }
}
