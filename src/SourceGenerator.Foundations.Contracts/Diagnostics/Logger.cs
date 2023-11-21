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
        private readonly List<ILogSink> m_sinks;

        public LogLevel Level { get; set; }

        public IReadOnlyList<ILogSink> Sinks => m_sinks;

        public Logger()
        {
            m_sinks = new List<ILogSink>();
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
            if(exception != null)
            {
                message += $"\n{exception}";
            }
            foreach(var sink in m_sinks)
            {
                sink.Write(logLevel, message);
            }
        }
    }
}
