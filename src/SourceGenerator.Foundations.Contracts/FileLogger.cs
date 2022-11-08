using SGF.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SGF
{
    /// <summary>
    /// An implemention of <see cref="ILogger"/> that logs to a file on disk
    /// 
    /// </summary>
    internal class FileLogger : ILogger
    {
        private readonly FileStream m_fileStream;
        private readonly StreamWriter m_writer;

        /// <summary>
        /// Gets the path to the log file
        /// </summary>
        public string FilePath { get; }

        public FileLogger(string context)
        {
            string logDirectory = Path.GetTempPath();
            logDirectory = Path.Combine(logDirectory, "SourceGenerator.Foundations");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            FilePath = Path.Combine(logDirectory, $"{context}.log");
            m_fileStream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            m_writer = new StreamWriter(m_fileStream, Encoding.UTF8);
            m_writer.AutoFlush = true;
            Log(LogLevel.Verbose, null, "Initializing {0}", nameof(FileLogger));
        }

        /// <inheritdoc cref="ILogger"/>
        public void Log(LogLevel logLevel, Exception? exception, string messageTemplate, params object[] arguments)
        {
            string renderedMessage = messageTemplate;

            try
            {
                renderedMessage = string.Format(messageTemplate, arguments);
                
            }
            catch (Exception formatException)
            {
                m_writer.WriteLine($"Exception thrown while rendering {formatException}");
            }
            string exceptionMessage = exception == null
                ? ""
                : $"\n{exception}";

            m_writer.WriteLine($"{DateTime.Now:hh:mm:ss} [{logLevel.GetMoniker()}] {renderedMessage}{exceptionMessage}");
        }
    }
}
