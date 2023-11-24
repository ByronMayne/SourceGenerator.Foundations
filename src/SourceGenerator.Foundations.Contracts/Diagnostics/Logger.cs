using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Implemention of a logger with custom sinks
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string m_sourceContext;
        private readonly List<ILogSink> m_sinks;

        public LogLevel Level { get; set; }

        public IReadOnlyList<ILogSink> Sinks => m_sinks;

        public Logger(string sourceContext)
        {
            m_sinks = new List<ILogSink>();
            m_sourceContext = sourceContext;
        }

        /// <summary>
        /// Adds a new logging sink
        /// </summary>
        public void AddSink(ILogSink sink)
        {
            m_sinks.Add(sink);
        }

        public bool IsEnabled(LogLevel level)
            => (int)Level >= (int)level;

        public void Log(LogLevel logLevel, Exception? exception, string message)
        {
            StringBuilder builder = new StringBuilder()
            .AppendFormat("[{0}] ", DateTime.Now.ToString("HH:mm:ss"))
            .AppendFormat("{0} | ", m_sourceContext);

            switch (logLevel)
            {
                case LogLevel.Warning:
                    builder.Append("Warning: ");
                    break;
                case LogLevel.Error:
                    builder.Append("Error: ");
                    break;
            }

            if(exception != null) builder.AppendLine(exception.ToString());

            string renderedMessage = builder.ToString();

            foreach(ILogSink sink in m_sinks)
            {
                sink.Write(logLevel, renderedMessage);
            }
        }
    }
}
