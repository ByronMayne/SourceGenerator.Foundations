using System;
using System.Collections.Generic;

namespace SGF.Diagnostics
{
    /// <summary>
    /// A logger used to write events for source generators
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets the log level configuration
        /// </summary>
        LogLevelConfiguration Configuration { get; }


        /// <summary>
        /// Gets the list of sinks that this logger writes to
        /// </summary>
        IReadOnlyList<ILogSink> Sinks { get; }

        /// <summary>
        /// Gets if the log level is enabled or not
        /// </summary>
        /// <param name="level"></param>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Adds a new sink to the logger
        /// </summary>
        /// <param name="sink">The sink to output the logs too</param>
        void AddSink(ILogSink sink);

        /// <summary>
        /// Logs an event to the logger
        /// </summary>
        /// <param name="logLevel">The level to log</param>
        /// <param name="exception">An optional exception that this log refers too</param>
        /// <param name="message">The message to output</param>
        void Log(LogLevel logLevel, Exception? exception, string message);

        /// <summary>
        /// Creates a new logger with the context of the type T, this is used to provide more information about where the log is coming from
        /// </summary>
        ILogger ForContext<T>();
    }
}
