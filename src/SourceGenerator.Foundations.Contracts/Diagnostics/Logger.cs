using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Diagnostics
{
    /// <summary>
    /// Implementation of a logger with custom sinks
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string m_sourceContext;
        private readonly string? m_contextName;
        private readonly List<ILogSink> m_sinks;
        private readonly LogLevelConfiguration m_configuration;

        [Obsolete("Use Configuration.GlobalLevel instead")]
        public LogLevel Level { get; set; }

        public IReadOnlyList<ILogSink> Sinks => m_sinks;

        /// <summary>
        /// Gets the log level configuration
        /// </summary>
        public LogLevelConfiguration Configuration => m_configuration;

        public Logger(string sourceContext)
        {
            m_sinks = new List<ILogSink>();
            m_sourceContext = sourceContext;
            m_configuration = new LogLevelConfiguration();
            m_contextName = null;
        }

        private Logger(string sourceContext, string? contextName, ILogger baseLogger)
        {
            if (baseLogger is not Logger logger)
                throw new ArgumentException("Base logger must be of type Logger", nameof(baseLogger));

            m_sinks = logger.m_sinks;
            m_configuration = logger.m_configuration;
            m_sourceContext = sourceContext;
            m_contextName = contextName;
        }

        /// <summary>
        /// Adds a new logging sink
        /// </summary>
        public void AddSink(ILogSink sink)
        {
            m_sinks.Add(sink);
        }


        public bool IsEnabled(LogLevel level)
            => m_configuration.IsEnabled(m_contextName, level);

        public ILogger ForContext<T>()
        {
            string contextName = typeof(T).Name;
            string sourceContextName = $"{m_sourceContext}:{contextName}";
            Logger logger = new Logger(sourceContextName, contextName, this);
            return logger;
        }

        public void Log(LogLevel logLevel, Exception? exception, string message)
        {
            if (!IsEnabled(logLevel))
                return;

            StringBuilder builder = new StringBuilder()
            .AppendFormat("[{0}] ", DateTime.Now.ToString("HH:mm:ss"))
            .AppendFormat("{0} | ", m_sourceContext);

            switch (logLevel)
            {
                case LogLevel.Trace:
                    builder.Append("Trace: ");
                    break;
                case LogLevel.Debug:
                    builder.Append("Debug: ");
                    break;
                case LogLevel.Information:
                    builder.Append("Info: ");
                    break;
                case LogLevel.Warning:
                    builder.Append("Warning: ");
                    break;
                case LogLevel.Error:
                    builder.Append("Error: ");
                    break;
            }

            builder.Append(message);

            if (exception != null) builder.AppendLine(exception.ToString());

            string renderedMessage = builder.ToString();

            foreach (ILogSink sink in m_sinks)
            {
                sink.Write(logLevel, renderedMessage);
            }
        }
    }
}
