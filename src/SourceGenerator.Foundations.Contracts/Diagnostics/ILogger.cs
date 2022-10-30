using System;
using System.Collections.Generic;
using System.Text;

namespace SGF.Diagnostics
{

    /// <summary>
    /// Interface used for logging information out from a process 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Creates a new log entry that will be shown by your IDE
        /// </summary>
        /// <param name="logLevel">The level of the log</param>
        /// <param name="exception">The exception if any</param>
        /// <param name="messageTemplate">The message template</param>
        /// <param name="arguments">Arguments that should be used for rendering</param>
        void Log(LogLevel logLevel, Exception? exception, string messageTemplate, object[] arguments);
    }
}
